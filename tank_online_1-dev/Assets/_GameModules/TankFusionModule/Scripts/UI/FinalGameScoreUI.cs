using Fusion.Utility;
using FusionHelpers;
using UnityEngine;
using TMPro;

namespace Fusion.TankOnlineModule
{
	public class FinalGameScoreUI : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer _crown;
		[SerializeField] private TextMeshPro _score;
		[SerializeField] private TextMeshPro _playerName;

		public void SetPlayerName(Player player)
		{
			_playerName.text = $"{player.PlayerName}";

			_score.color = Color.red;
			_playerName.color = Color.red;
		}

		public void SetScore(int newScore)
		{
			_score.text = newScore.ToString();
		}

		public void ToggleCrown(bool on)
		{
			_crown.enabled = on;
		}
	}
}