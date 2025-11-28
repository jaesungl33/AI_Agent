using UnityEngine;
using System.Globalization;
using System.Collections;
using UnityEngine.Networking;
using System.Threading;
using Fusion;
using System;
using Fusion.GameSystems;
using System.Collections.Generic;

public class RegionDetection : MonoBehaviour
{
    public NetworkRunner runnerPrefab;
    private NetworkRunner runnerInstance;
    [Serializable]
    public class RegionInfo
    {
        public string RegionCode;
        public int RegionPing;
    }
    private CancellationTokenSource _tokenSource;
    public List<RegionInfo> regionList = new List<RegionInfo>();

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        runnerInstance = Instantiate(runnerPrefab);
        DontDestroyOnLoad(runnerInstance.gameObject);
        RefreshRegionDropdown();
    }

    private void OnDestroy()
    {
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
    }

    public async void RefreshRegionDropdown()
    {
        // Cancel token cũ nếu có
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();

        _tokenSource = new CancellationTokenSource();
        regionList.Clear();

        try
        {
            var regions = await NetworkRunner.GetAvailableRegions(cancellationToken: _tokenSource.Token);

            if (regions != null)
            {
                for (int i = 0; i < regions.Count; i++)
                {
                    var region = regions[i];
                    regionList.Add(new RegionInfo { RegionCode = region.RegionCode, RegionPing = region.RegionPing });
                }
            }
            ChooseBestRegion();
            _ = runnerInstance.Shutdown();
            Destroy(runnerInstance.gameObject);
            Destroy(this.gameObject);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("RefreshRegionDropdown cancelled.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error refreshing regions: {ex}");
        }
    }

    private void ChooseBestRegion()
    {
        if (regionList == null || regionList.Count == 0)
            return;

        int bestIndex = 0;
        int bestPing = int.MaxValue;

        for (int i = 0; i < regionList.Count; i++)
        {
            int ping = regionList[i].RegionPing;
            if (ping < bestPing)
            {
                if (ping < bestPing)
                {
                    bestPing = ping;
                    bestIndex = i;
                }
            }
        }
        GameManager.fixedRegion = regionList[bestIndex].RegionCode;
    }
}
