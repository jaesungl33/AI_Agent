using System;
using Fusion.UIHelpers;
using Fusion.Utility;
using TMPro;
using UnityEngine;

namespace Tanknarok.UI
{
	[RequireComponent(typeof(Panel))]
	public class ErrorBox : Singleton<ErrorBox>
	{
		[SerializeField] private TMP_Text _header;
		[SerializeField] private TMP_Text _message;

		private Action _onClose;
		[SerializeField] private Panel _panel;

		public static void Show(string header, string message, Action onclose)
		{
			Singleton<ErrorBox>.Instance.ShowInternal(header, message, onclose);
		}

		private void ShowInternal(string header, string message, Action onclose)
		{
			if(_panel==null)
				_panel = GetComponent<Panel>();
			_panel.SetVisible(true);

			_header.text = header;
			_message.text = message;
			_onClose = onclose;
	
		}

		public void OnClose()
		{
			_panel.SetVisible(false);
			_onClose?.Invoke();
		}
	}
}