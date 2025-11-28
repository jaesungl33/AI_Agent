# LevelManager Analysis - Photon Fusion 2 Compliance

## 🔍 Issues Found:

### ❌ **CRITICAL ISSUES:**

1. **Async/await with Runner.LoadScene/UnloadScene is problematic**
   ```csharp
   // Current Code (Line 109-119) - WRONG:
   private async void LoadLevel(LevelEvent @event)
   {
       if(Runner != null && Runner.IsSharedModeMasterClient)
       {
           _currentLevel = null;
           if (_loadedScene.IsValid)
           {
               await Runner.UnloadScene(_loadedScene);
               _loadedScene = SceneRef.None;
           }
           await Runner.LoadScene(SceneRef.FromIndex(@event.sceneIndex), 
               new UnityEngine.SceneManagement.LoadSceneParameters(@event.loadSceneMode), true);
       }
   }
   ```
   
   **Problem:** 
   - `async void` is dangerous - exceptions cannot be caught
   - Scene loading should be handled via coroutines or properly awaited Tasks
   - No error handling
   
2. **Missing OnSceneLoaded override signature**
   ```csharp
   // Current signature (Line 171):
   protected override IEnumerator OnSceneLoaded(SceneRef newScene, Scene loadedScene, NetworkLoadSceneParameters sceneFlags)
   
   // Correct Fusion 2 signature should be:
   protected override IEnumerator OnSceneLoaded(SceneRef sceneRef, Scene scene, NetworkLoadSceneParameters sceneParams)
   ```
   - Parameter names matter for clarity

3. **Not using base class properly**
   - Should call base methods at correct points
   - Missing proper error handling

### ⚠️ **WARNINGS:**

4. **Direct SceneManager calls mixed with Fusion**
   ```csharp
   // Line 113 - Creating LoadSceneParameters directly
   new UnityEngine.SceneManagement.LoadSceneParameters(@event.loadSceneMode)
   ```
   - Should use NetworkLoadSceneParameters properly

5. **Authority checks incomplete**
   ```csharp
   // Line 109:
   if(Runner != null && Runner.IsSharedModeMasterClient)
   ```
   - Should also check `Runner.IsServer` for other game modes
   - Better: `if (Runner != null && (runner.IsSharedModeMasterClient))`

6. **Scene state not tracked properly**
   ```csharp
   _loadedScene = SceneRef.None; // Line 191
   ```
   - Setting to None immediately after load request, but load is async

7. **FindAnyObjectByType deprecated warning**
   ```csharp
   // Line 195:
   _currentLevel = FindAnyObjectByType<LevelBehaviour>();
   ```
   - Modern Unity uses `FindFirstObjectByType<T>()`

8. **Missing NetworkSceneAsyncOp handling**
   - Not storing or checking the async operation status
   - No cancellation support

## ✅ **RECOMMENDED FIXES:**

### Fix 1: Proper Scene Loading with Error Handling
```csharp
private void LoadLevel(LevelEvent @event)
{
    if (Runner != null && (runner.IsSharedModeMasterClient))
    {
        StartCoroutine(LoadLevelCoroutine(@event));
    }
}

private IEnumerator LoadLevelCoroutine(LevelEvent @event)
{
    _currentLevel = null;
    
    // Unload previous scene
    if (_loadedScene.IsValid)
    {
        Debug.Log($"Unloading previous scene: {_loadedScene}");
        var unloadOp = Runner.UnloadScene(_loadedScene);
        
        while (!unloadOp.IsDone)
        {
            yield return null;
        }
        
        if (!unloadOp.IsValid)
        {
            Debug.LogError($"Failed to unload scene {_loadedScene}");
            yield break;
        }
        
        _loadedScene = SceneRef.None;
    }
    
    // Load new scene
    Debug.Log($"Loading new scene: {@event.sceneIndex}");
    var sceneRef = SceneRef.FromIndex(@event.sceneIndex);
    var loadParams = new NetworkLoadSceneParameters
    {
        LoadSceneMode = @event.loadSceneMode,
        LocalPhysicsMode = LocalPhysicsMode.None
    };
    
    var loadOp = Runner.LoadScene(sceneRef, loadParams);
    
    while (!loadOp.IsDone)
    {
        yield return null;
    }
    
    if (!loadOp.IsValid)
    {
        Debug.LogError($"Failed to load scene {sceneRef}");
        yield break;
    }
    
    _loadedScene = sceneRef;
}
```

### Fix 2: Proper OnSceneLoaded Override
```csharp
protected override IEnumerator OnSceneLoaded(SceneRef sceneRef, Scene scene, NetworkLoadSceneParameters sceneParams)
{
    Debug.Log($"LevelManager.OnSceneLoaded({sceneRef}, {scene.name}, {sceneParams})");

    // Call base implementation FIRST
    yield return base.OnSceneLoaded(sceneRef, scene, sceneParams);

    if (sceneRef.AsIndex == 0)
        yield break;

    _transitionEffect.ToggleGlitch(true);
    _audioEmitter.Play();

    yield return null;

    _loadedScene = sceneRef;
    Debug.Log($"Scene loaded: {sceneRef}");

    // Delay one frame
    yield return null;

    // Activate the next level
    _currentLevel = FindFirstObjectByType<LevelBehaviour>(); // Use FindFirstObjectByType
    if (_currentLevel != null)
    {
        _currentLevel.Activate();
    }
    else
    {
        Debug.LogWarning($"No LevelBehaviour found in scene {scene.name}");
    }

    yield return new WaitForSeconds(0.3f);

    Debug.Log("Stop glitching");
    _transitionEffect.ToggleGlitch(false);
    _audioEmitter.Stop();

    // Wait for CoreGamePlay
    CoreGamePlay gameManager = null;
    int attempts = 0;
    while (gameManager == null && attempts < 100)
    {
        Runner.TryGetSingleton(out gameManager);
        if (gameManager == null)
        {
            yield return null;
            attempts++;
        }
    }

    if (gameManager == null)
    {
        Debug.LogError("Failed to find CoreGamePlay after 100 attempts!");
        yield break;
    }

    if (gameManager.matchWinner != null && sceneRef.AsIndex == _lobby)
    {
        _scoreManager.ShowFinalGameScore(gameManager);
    }

    gameManager.lastPlayerStanding = null;

    // Respawn players
    Debug.Log($"Respawning {gameManager.PlayerCount} Players");
    var players = gameManager.AllPlayers.ToArray();
    foreach (var fusionPlayer in players)
    {
        if (fusionPlayer is Player player)
        {
            player.Reset();
            player.Respawn();
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    Debug.Log($"All Players Respawned in scene {_loadedScene.AsIndex}");
}
```

### Fix 3: Better Shutdown Implementation
```csharp
public override void Shutdown()
{
    Debug.Log("LevelManager.Shutdown()");
    
    _currentLevel = null;
    InputController.fetchInput = false;
    
    // Let base class handle scene cleanup
    base.Shutdown();
    
    // Clear local state AFTER base shutdown
    _loadedScene = SceneRef.None;
}
```

### Fix 4: Safe UnloadSceneCoroutine
```csharp
protected override IEnumerator UnloadSceneCoroutine(SceneRef prevScene)
{
    Debug.Log($"LevelManager.UnloadSceneCoroutine({prevScene})");

    // Wait for CoreGamePlay
    CoreGamePlay coreGameplay = null;
    int attempts = 0;
    while (coreGameplay == null && attempts < 100)
    {
        Runner.TryGetSingleton(out coreGameplay);
        if (coreGameplay == null)
        {
            yield return null;
            attempts++;
        }
    }

    if (coreGameplay == null)
    {
        Debug.LogError("CoreGamePlay not found during unload!");
        yield return base.UnloadSceneCoroutine(prevScene);
        yield break;
    }

    if (prevScene.AsIndex > 0)
    {
        yield return new WaitForSeconds(1.0f);

        InputController.fetchInput = false;

        // De-spawn all tanks
        Debug.Log("De-spawning all tanks");
        var playersSnapshot = coreGameplay.AllPlayers.ToArray();

        foreach (var fusionPlayer in playersSnapshot)
        {
            if (fusionPlayer is Player player)
            {
                Debug.Log($"De-spawning tank {player.NetPlayerIndex}");
                player.TeleportOut();
                yield return new WaitForSeconds(0.1f);
            }
        }

        yield return new WaitForSeconds(1.5f - coreGameplay.PlayerCount * 0.1f);

        _scoreManager.ResetAllGameScores();
        
        if (coreGameplay.lastPlayerStanding != null)
        {
            _scoreManager.ShowIntermediateLevelScore(coreGameplay);
            yield return new WaitForSeconds(1.5f);
            _scoreManager.ResetAllGameScores();
        }
    }

    // Always call base implementation
    yield return base.UnloadSceneCoroutine(prevScene);
}
```

## 📋 **Best Practices Checklist:**

- ✅ Always call `base.OnSceneLoaded()` and `base.UnloadSceneCoroutine()`
- ✅ Use coroutines instead of `async void` for scene operations
- ✅ Check `NetworkSceneAsyncOp.IsDone` and `IsValid` 
- ✅ Use `NetworkLoadSceneParameters` instead of Unity's `LoadSceneParameters`
- ✅ Handle both Server and SharedModeMasterClient
- ✅ Add timeout/retry logic when waiting for singletons
- ✅ Use `FindFirstObjectByType` instead of `FindAnyObjectByType`
- ✅ Proper null checks and error logging
- ✅ Clean state in correct order during Shutdown

## 🎯 **Performance Improvements:**

1. Cache scene references properly
2. Use object pooling for frequently spawned objects
3. Avoid FindObjectOfType in Update loops
4. Snapshot collections before iterating during despawn

## 🔒 **Thread Safety:**

- Fusion scene operations are NOT thread-safe
- Always use coroutines on main thread
- Avoid `async void` completely
- Use `async Task` only if you can properly await it
