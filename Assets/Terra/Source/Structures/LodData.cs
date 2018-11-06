﻿using System;
using Terra.Terrain;
using UnityEngine;

namespace Terra.Structure {
	[Serializable]
	public class LodData {
		public enum LodLevelType {
			High, Medium, Low
		}

		/// <summary>
		/// Represents a level of detail for each <see cref="TileMesh"/> to 
		/// adhere to. This specifies the resolution of various maps across 
		/// <see cref="Tile"/>s.
		/// 
		/// The resolution of <see cref="SplatmapResolution"/> and 
		/// <see cref="MeshResolution"/> cannot be greater than 
		/// <see cref="MapResolution"/>.
		/// </summary>
		[Serializable]
		public class LodLevel {
			[SerializeField]
			private int _mapRes;

			/// <summary>
			/// Where on the circular grid does this LOD level start to appear?
			/// </summary>
			public int StartRadius;

			public static bool operator <(LodLevel lhs, LodLevel rhs) {
				return lhs._mapRes < rhs._mapRes;
			}

			public static bool operator >(LodLevel lhs, LodLevel rhs) {
				return lhs._mapRes > rhs._mapRes;
			}

			public static bool operator <=(LodLevel lhs, LodLevel rhs) {
				return lhs._mapRes <= rhs._mapRes;
			}

			public static bool operator >=(LodLevel lhs, LodLevel rhs) {
				return lhs._mapRes >= rhs._mapRes;
			}

			/// <summary>
			/// Resolution of the height, moisture, and temperature maps for 
			/// each <see cref="Tile"/>.
			/// </summary>
			public int MapResolution {
				get { return _mapRes; }
				set {
					_mapRes = value;
					VerifyResolutions();
				}
			}

			public LodLevel(int startRadius, int mapRes) {
				StartRadius = startRadius;
				_mapRes = mapRes;

				VerifyResolutions();
			}

			private void VerifyResolutions() {
				_mapRes = Mathf.ClosestPowerOfTwo(_mapRes) + 1;
			}
		}

		[SerializeField]
		private bool _useLowLod;
		[SerializeField]
		private bool _useMediumLod;
		[SerializeField]
		private bool _useHighLod = true;

		public bool UseLowLodLevel {
			get { return _useLowLod; }
			set { _useLowLod = value; VerifyLodLevelEnabled(); }
		}
		public bool UseMediumLodLevel {
			get { return _useMediumLod; }
			set { _useMediumLod = value; VerifyLodLevelEnabled(); }
		}
		public bool UseHighLodLevel {
			get { return _useHighLod; }
			set { _useHighLod = value; VerifyLodLevelEnabled(); }
		}

		public LodLevel Low = new LodLevel(2, 32);
		public LodLevel Medium = new LodLevel(1, 64);
		public LodLevel High = new LodLevel(0, 512);

		/// <summary>
		/// Get the LodLevel associated with the passed radius. If 
		/// no level matches, <see cref="High"/> is returned instead.
		/// </summary>
		/// <param name="radius">Radius to look for</param>
		public LodLevel GetLevelForRadius(int radius) {
			foreach (var lvl in new[]{ Low, Medium, High }) {
				if (lvl.StartRadius - 1< radius) {
					return lvl;
				}
			}

			return High;
		}

		/// <summary>
		/// Works exactly the same as <see cref="GetLevelForRadius"/> 
		/// but instead of returning the <see cref="LodLevel"/> itself, 
		/// the <see cref="LodLevelType"/> is returned.
		/// </summary>
		/// <param name="radius">Radius to look for</param>
		public LodLevelType GetLevelTypeForRadius(int radius) {
			if (UseLowLodLevel && Low.StartRadius - 1 < radius)
				return LodLevelType.Low;
			if (UseMediumLodLevel && Medium.StartRadius - 1 < radius)
				return LodLevelType.Medium;

			return LodLevelType.High;
		}
		
		/// <summary>
		/// Ensures that at least one LOD level is enabled at a time. 
		/// If all levels are false, <see cref="UseHighLodLevel"/> is 
		/// made true.
		/// </summary>
		private void VerifyLodLevelEnabled() {
			if (!UseLowLodLevel && !UseMediumLodLevel && !UseHighLodLevel) {
				UseHighLodLevel = true;
			}
		}
	}
}
