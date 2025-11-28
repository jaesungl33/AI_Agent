using UnityEngine;
using UnityEngine.Serialization;

namespace GDOLib.UI
{
    public class ObjectPresenter : MonoBehaviour, IPresenter
    {
        [SerializeField, FormerlySerializedAs("ID")] private string id;
        private string _idCache;
        public string ID
        {
            get
            {
                if(string.IsNullOrEmpty(id))
                {
                    if(_idCache != gameObject.name.ToLowerInvariant())
                    {
                        _idCache = gameObject.name.ToLowerInvariant();
                    }
                    return _idCache;
                }
                return id;
            }
            set => id = value;
        }
    }
}