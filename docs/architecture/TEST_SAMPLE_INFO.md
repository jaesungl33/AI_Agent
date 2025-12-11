# Test Code Sample - tank_online_1-dev

## Selected Files (6 files, 70 KB)

This test sample contains representative C# files from different game modules:

### Core Game Logic
1. **GameManager.cs** - `Assets/_GamePlay/Scripts/GameManager.cs`
   - Main game manager/controller

2. **SceneLoader.cs** - `Assets/_GamePlay/Scripts/SceneLoader.cs`
   - Scene loading and management

### UI Components
3. **HomeScreen.cs** - `Assets/_GameUI/MainScreen/Scripts/HomeScreen.cs`
   - Main menu/home screen UI

4. **ChooseTankScreen.cs** - `Assets/_GameUI/LobbyScreen/Scripts/ChooseTankScreen.cs`
   - Tank selection screen

### Game Modules
5. **LanguageManager.cs** - `Assets/_GameModules/LocalizationModule/Scripts/LanguageManager.cs`
   - Localization/i18n system

6. **UserDataCollectionBase.cs** - `Assets/_GameModules/GameDataModule/Scripts/UserDataCollectionBase.cs`
   - User data management

## File Location
- **Source**: `/Users/madeinheaven/Documents/GitHub/AI_Agent/docs/tank_online_test_sample.txt`
- **Size**: 70 KB
- **Doc ID**: `tank_online_test_sample`

## Next Steps

### Option 1: Upload via Web UI
1. Open the frontend at http://localhost:3000
2. Go to Upload page
3. Select "Code" tab
4. Upload `docs/tank_online_test_sample.txt`
5. Set index ID: `tank_online_test_sample`

### Option 2: Use Backend API (when backend is responsive)
```bash
curl -X POST http://127.0.0.1:8000/documents/code \
  -F "file=@docs/tank_online_test_sample.txt" \
  -F "indexId=tank_online_test_sample"
```

### Option 3: Test Coverage Evaluation
Once indexed, you can test coverage evaluation with:
- GDD: Any of your indexed GDD documents
- Code: `tank_online_test_sample`

## Note
The .txt file may have indexing issues due to raganything parser limitations.
If indexing fails, the behavior indexing system can still process the code
for coverage evaluation once chunks are available.
