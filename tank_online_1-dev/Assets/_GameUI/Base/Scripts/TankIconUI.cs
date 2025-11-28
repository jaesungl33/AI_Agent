using UnityEngine;

public class TankIconUI : MonoBehaviour
{
    [SerializeField] private Transform _container;

    private GameObject _tankPreview;
    
    public void SetTank(string tankId)
    {
        if (_tankPreview != null)
        {
            Destroy(_tankPreview);
            _tankPreview = null;
        }
        
        DatabaseManager.GetDB<GameAssetCollection>(result =>
        {
            if (result != null)
            {
                var tankPrefab = result.GetGameAssetDocumentById(tankId).previewAsset as GameObject;
                _tankPreview = Instantiate(tankPrefab, Vector3.zero, Quaternion.identity, _container);
                _tankPreview.transform.SetParent(_container);
                _tankPreview.transform.localPosition = Vector3.zero;
                _tankPreview.transform.localRotation = Quaternion.identity;
                _tankPreview.transform.localScale = Vector3.one;
                _tankPreview.transform.localPosition = Vector3.zero;
                _tankPreview.transform.localRotation = Quaternion.identity;
                _tankPreview.transform.localScale = Vector3.one;
            }
        });
    }
}