using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(ProductCollection), menuName = "ScriptableObjects/" + nameof(ProductCollection), order = 2)]
public class ProductCollection : CollectionBase<ProductDocument>
{
}
