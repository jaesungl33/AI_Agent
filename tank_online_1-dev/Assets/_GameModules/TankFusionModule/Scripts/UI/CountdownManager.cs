using System.Collections;
using UnityEngine;
using TMPro;

namespace Fusion.TankOnlineModule
{
	/// <summary>
	/// CountdownManager handles the countdown before the game starts.
	/// It displays the countdown on the UI and plays a sound effect.
	/// </summary>
	public class CountdownManager : MonoBehaviour
	{
		[SerializeField] private float _countdownFrom;
		[SerializeField] private AnimationCurve _countdownCurve;
		[SerializeField] private TextMeshProUGUI _countdownUI;
		[SerializeField] GDO.Audio.AudioEmitter _audioEmitter;
		[SerializeField] private GameObject attackObj, defenseObj;
		private float _countdownTimer;

		public delegate void Callback();

		private void Start()
		{
			Reset();
		}

		public void Reset()
		{
			_countdownUI.transform.localScale = Vector3.zero;
		}

		public IEnumerator Countdown(Callback callback)
		{
			_countdownUI.transform.localScale = Vector3.zero;

			_countdownUI.text = _countdownFrom.ToString();
			_countdownUI.gameObject.SetActive(true);
			attackObj.gameObject.SetActive(false);
			defenseObj.gameObject.SetActive(false);
			int lastCount = Mathf.CeilToInt(_countdownFrom + 1);
			_countdownTimer = _countdownFrom;

			while (_countdownTimer > 0)
			{
				int currentCount = Mathf.CeilToInt(_countdownTimer);

				if (lastCount != currentCount)
				{
					lastCount = currentCount;
					_countdownUI.text = currentCount.ToString();
					_audioEmitter.PlayOneShot();
				}

				float x = _countdownTimer - Mathf.Floor(_countdownTimer);

				float t = _countdownCurve.Evaluate(x);
				if (t >= 0)
					_countdownUI.transform.localScale = Vector3.one * t;

				_countdownTimer -= Time.deltaTime * 1.5f;
				yield return null;
			}
			
			_countdownUI.text = "";
			_countdownUI.gameObject.SetActive(false);
			yield return new WaitForSeconds(0.1f);
			var teamIndex = DatabaseManager.GetDB<SOMatchData>().GetLocalTeamIndex();
			if (teamIndex == 0)
				defenseObj.gameObject.SetActive(true);
			else if (teamIndex == 1)
				attackObj.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.5f);
			defenseObj.gameObject.SetActive(false);
			attackObj.gameObject.SetActive(false);
			callback?.Invoke();
		}
	}
}