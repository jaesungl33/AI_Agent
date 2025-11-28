using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GDOLib.Components
{
    public class GrayScaleImage : MonoBehaviour
    {
        [SerializeField] private Image _image;

        [SerializeField] Image[] images;

        private Material _material;
        private Material _grayScaleMaterial;
        private Dictionary<Image, Color> _originalColor = new Dictionary<Image, Color>();
        private bool isApplicationQuitting;

        private void Awake()
        {
            if (images == null)
            {
                images = GetComponentsInChildren<Image>();
            }
            else
            {
                images = images.Where(i => i != null).ToArray();
            }

            if (images != null)
            {
                //foreach (Image image in images)
                //{
                //    if(image != null)
                //    {
                //        _originalColor.Add(image, image.color); 
                //    }
                //}
                _originalColor = images.Where(i => i != null).ToDictionary(i => i, i => i.color);
            }
        }

        private void OnValidate()
        {
            _image = GetComponent<Image>();

            if (images == null)
            {
                images = GetComponentsInChildren<Image>();
            }
        }

        private void OnEnable()
        {
            if (_image == null)
                return;

            SetGrayScale(true);
        }

        private void OnDisable()
        {
            if (_image == null)
                return;

            if (!isApplicationQuitting)
                SetGrayScale(false);
        }

        private void OnApplicationQuit()
        {
            isApplicationQuitting = true;
        }

        public void UpdateGrayScale()
        {
            SetGrayScale(enabled);
        }

        public void SetGrayScale(bool isGrayScale)
        {
            //Debug.Log($"SetGrayScale {transform.parent.name}/{name}: {isGrayScale}");
            if (_material == null)
            {
                _material = new Material(_image.material);
                _grayScaleMaterial = new Material(_material);
                _grayScaleMaterial.name += _grayScaleMaterial.GetHashCode();
                _material.SetFloat("_GrayscaleAmount", 0f);
                _grayScaleMaterial.SetFloat("_GrayscaleAmount", 1f);

                if (_originalColor.Count != images.Length)
                {
                    _originalColor = images.Where(i => i != null).ToDictionary(i => i, i => i.color);
                }

                _image.material = _grayScaleMaterial;
                if (images != null)
                {
                    for (int i = 0; i < images.Length; i++)
                    {
                        var image = images[i];
                        image.material = _grayScaleMaterial;
                    }
                }
            }

            var targetMaterial = isGrayScale ? _grayScaleMaterial : _material;
            _image.material = targetMaterial;
            if (images != null)
            {
                for (int i = 0; i < images.Length; i++)
                {
                    var image = images[i];
                    image.material = targetMaterial;
                }
            }

            for (int i = 0; i < images.Length; i++)
            {
                var image = images[i];
                var grayScaleColor = _originalColor[image];
                var avgColor = (grayScaleColor.r + grayScaleColor.g + grayScaleColor.b) / 3f;
                grayScaleColor = new Color(avgColor, avgColor, avgColor, grayScaleColor.a);
                image.color = isGrayScale ? grayScaleColor : _originalColor[image];
                image.SetAllDirty();
                _image.SetMaterialDirty();
            }
        }
        
        public void SetGrayScale(float value)
        {
            var targetMaterial = new Material(_grayScaleMaterial);
            targetMaterial.SetFloat("_GrayscaleAmount", value);
            _image.material = targetMaterial;
            if (images != null)
            {
                for (int i = 0; i < images.Length; i++)
                {
                    var image = images[i];
                    image.material = targetMaterial;
                }
            }

            for (int i = 0; i < images.Length; i++)
            {
                var image = images[i];
                var grayScaleColor = _originalColor[image];
                var avgColor = (grayScaleColor.r + grayScaleColor.g + grayScaleColor.b) / 3f;
                grayScaleColor = new Color(avgColor, avgColor, avgColor, grayScaleColor.a);
                image.color = Color.Lerp(_originalColor[image], grayScaleColor, value);
                image.SetAllDirty();
                _image.SetMaterialDirty();
            }
        }
    }
}