﻿using System;
using UnityEngine;
using Terra.Structure;

namespace Terra.Terrain {
	/// <summary>
	///	Tile represents a Terrain gameobject in the scene. 
	///	This class handles the instantiation of Terrain, noise, 
	///	position, texture, and detail application.
	/// </summary>
	[ExecuteInEditMode]
	public class Tile: MonoBehaviour, ISerializationCallbackReceiver {
		private TerraConfig Config { get { return TerraConfig.Instance; } }

		[SerializeField]
		private TilePaint _painter;
		[SerializeField]
		private TileMesh _meshManager;
		[SerializeField]
		private LodData.LodLevel _lastGeneratedLodLevel;

		[HideInInspector]
		public bool IsColliderDirty = false;

		/// <summary>
		/// Position of this Tile in the grid of Tiles
		/// </summary>
		public GridPosition GridPosition { get; private set; }

		/// <summary>
		/// Create and manage mesh(es) attached to this Tile. This 
		/// provides an interface for creating and showing meshes of 
		/// varying resolutions.
		/// </summary>
		public TileMesh MeshManager {
			get {
				if (_meshManager == null) {
					_meshManager = new TileMesh(this, GetLodLevel());
				}

				return _meshManager;
			}
			set {
				_meshManager = value;
			}
		}

		/// <summary>
		/// Handles "painting" of this Tile through a splatmap that is 
		/// applied to each MeshRenderer.
		/// </summary>
		public TilePaint Painter {
			get {
				if (_painter == null) {
					_painter = new TilePaint(this);
				}

				return _painter;
			}
			set {
				_painter = value;
			}
		}

		/// <summary>
		/// The LOD level for this Tile. This value can change if the tracked object 
		/// has moved or <see cref="GridPosition"/> was modified.
		/// </summary>
		public LodData.LodLevel LodLevel {
			get {
				return GetLodLevel();
			}
		}

		/// <summary>
		/// The LOD level of this mesh during the last call to <see cref="Generate"/>. If 
		/// <see cref="Generate"/> hasn't been called this value is null.
		/// </summary>
		public LodData.LodLevel LastGeneratedLodLevel {
			get {
				return _lastGeneratedLodLevel;
			}
		}

		/// <summary>
		/// Creates a gameobject with an attached Tile component and 
		/// places it in the scene. This is a convienence method and is not required 
		/// for correct tile creation.
		/// </summary>
		/// <param name="name">Name of the created gameobject</param>
		/// <returns>The attached Tile component</returns>
		public static Tile CreateTileGameobject(string name) {
			GameObject go = new GameObject(name);
			Tile tt = go.AddComponent<Tile>();

			return tt;
		}

		/// <summary>
		/// Fully constructs this Tile. This includes creating a Mesh, painting 
		/// the terrain, and adding details (grass, objects, etc.)
		/// 
		/// By default, calculating heights is done off of the main thread but 
		/// can be disabled.
		/// </summary>
		/// <param name="onComplete">Called after all calculations have completed. 
		/// <see cref="onComplete"/>Can be null if the result is not needed.</param>
		/// <param name="async">Perform mesh computation asynchronously</param>
		public void Generate(Action onComplete, bool async = true) {
			//Cache current LOD
			_lastGeneratedLodLevel = GetLodLevel();
			MeshManager.LodLevel = _lastGeneratedLodLevel;

			if (async) {
				MeshManager.CreateHeightmapAsync(() => {
					MeshManager.CreateMesh();
					PostCreateMeshGenerate();

					if (onComplete != null) {
						onComplete();
					}
				});
			} else {
				MeshManager.CreateHeightmap();
				MeshManager.CreateMesh();
				PostCreateMeshGenerate();

				if (onComplete != null) {
					onComplete();
				}
			}
		}

		/// <summary>
		/// Updates this TerrainTiles position by taking a Vector2 where 
		/// the x and y values are integers on a grid. Internally the x and y values 
		/// are multiplied by the Length of the mesh specified in TerraSettings
		/// </summary> 
		/// <param name="position">Position to set the Tile to (ie [1,0])</param>
		/// <param name="transformInScene">Move this Tile's gameobject to match position change?</param>
		public void UpdatePosition(GridPosition position, bool transformInScene = true) {
			GridPosition = position;

			//Update TileMesh LOD level
			MeshManager.LodLevel = GetLodLevel();

			if (transformInScene) {
				int len = Config.Generator.Length;
				transform.position = new Vector3(position.X * len, 0f, position.Z * len);
			}
		} 

		/// <summary>
		/// Get the MeshFilter attached to this gameobject. If one doesn't 
		/// exist, it is added and returned.
		/// </summary>
		public MeshFilter GetMeshFilter() {
			MeshFilter mf = GetComponent<MeshFilter>();
			if (mf == null) {
				mf = gameObject.AddComponent<MeshFilter>();
			}

			return mf;
		}

		/// <summary>
		/// Get the MeshRenderer attached to this gameobject. If one doesn't 
		/// exist, it is added and returned.
		/// </summary>
		public MeshRenderer GetMeshRenderer() {
			MeshRenderer mr = GetComponent<MeshRenderer>();
			if (mr == null) {
				mr = gameObject.AddComponent<MeshRenderer>();
			}

			return mr;
		}

		/// <summary>
		/// Checks whether this Tile's heightmap matches its set level of detail.
		/// </summary>
		/// <returns>true if heightmap matches lod, false otherwise</returns>
		public bool IsHeightmapLodValid() {
			return LastGeneratedLodLevel >= GetLodLevel();
		}

		/// <summary>
		/// Finishes the <see cref="Generate"/> method after the 
		/// mesh has been created. This exists as a convenience as 
		/// a mesh can be created asynchronously or synchronously but 
		/// the logic afterwards is the same.
		/// </summary>
		private void PostCreateMeshGenerate() {
			Painter.Paint();
		}

		/// <summary>
		/// Gets the LOD level for this tile based off of its <see cref="GridPosition"/>'s 
		/// distance from the tracked object. If no tracked object is specified, the level 
		/// is determined by the <see cref="GridPosition"/>'s distance from [0, 0]. The returned 
		/// LOD level may not be equal to this Tile's <see cref="LodLevel"/> as it could have changed 
		/// since initialization.
		/// </summary>
		/// <returns>LOD level</returns>
		private LodData.LodLevel GetLodLevel() { //todo change to reflect description
			GameObject tracked = Config.Generator.TrackedObject;

			if (tracked == null) {
				int radius = (int)GridPosition.Distance(new GridPosition(0, 0));
				return Config.Generator.Lod.GetLevelForRadius(radius);
			} else {
				float length = Config.Generator.Length;
				Vector2 worldXZ = new Vector2(tracked.transform.position.x, tracked.transform.position.z);
				GridPosition gp = new GridPosition(worldXZ, length);

				int radius = (int)gp.Distance(GridPosition);
				return Config.Generator.Lod.GetLevelForRadius(radius);
			}
		}

		public override string ToString() {
			return "Tile[" + GridPosition.X + ", " + GridPosition.Z + "]";
		}

		#region Serialization

		[SerializeField]
		private GridPosition _serializedGridPosition;

		public void OnBeforeSerialize() {
			//Grid Position
			_serializedGridPosition = GridPosition;
		}

		public void OnAfterDeserialize() {
			//Grid Position
			GridPosition = _serializedGridPosition;
		}

		#endregion
	}
}