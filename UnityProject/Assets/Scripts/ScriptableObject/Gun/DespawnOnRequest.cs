﻿using UnityEngine;

namespace Weapons.Projectiles.Behaviours
{
	/// <summary>
	/// Requests to despawn game object only if a hit behaviour returns true
	/// </summary>
	[CreateAssetMenu(fileName = "DespawnOnRequest", menuName = "ScriptableObjects/Gun/DespawnOnRequest", order = 0)]
	public class DespawnOnRequest : HitProcessor
	{
		public override bool ProcessHit(RaycastHit2D hit, IOnHit[] behavioursOnBulletHit)
		{
			var isRequesting = false;
			foreach (var behaviour in behavioursOnBulletHit)
			{
				if (behaviour.OnHit(hit))
				{
					isRequesting = true;
				}
			}

			return isRequesting;
		}
	}
}