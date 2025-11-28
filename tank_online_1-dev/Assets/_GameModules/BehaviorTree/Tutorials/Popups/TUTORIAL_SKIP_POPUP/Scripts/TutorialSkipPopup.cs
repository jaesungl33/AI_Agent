using System;
using DG.Tweening;
using UnityEngine;
using GDOLib.UI;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UniRx;
using BehaviorTree.Tutorials;

namespace GDO.View
{
	public class TutorialSkipPopup : PopupBase
	{
		[SerializeField] private Button btnContinue;
		[SerializeField] private Button btnSkip;
		private TutorialSkipPopupParam param;

		protected override void Awake()
		{
			base.Awake();
			btnContinue.onClick.AddListener(OnClickContinue);
			btnSkip.onClick.AddListener(OnClickSkip);
		}
		public override void Show(int additionalSortingOrder = 0, ScreenParam screenParam = null)
		{
			base.Show(additionalSortingOrder, screenParam);
			param = screenParam as TutorialSkipPopupParam;
		}
		private void OnClickSkip()
        {
			param?.onSkip?.Invoke();
			ClosePopup();
        }

		private void OnClickContinue()
        {
			param?.onContinue?.Invoke();
			ClosePopup();
        }
	}
	public class TutorialSkipPopupParam : ScreenParam
	{
		public Action onSkip;
		public Action onContinue;
	}
}