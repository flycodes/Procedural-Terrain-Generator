﻿using System;
using System.Collections.Generic;
using Terra.CoherentNoise;
using Terra.Graph.Generators;
using Terra.Structures;
using Terra.Terrain;
using Terra.Util;
using UnityEngine;
using XNode;

namespace Terra.Graph.Biome {
    [Serializable]
    public abstract class DetailModifierNode : Node {
        [Output]
        public NodePort Output;

        public int GridSize = 100;

        public abstract Vector2[] SamplePositions(Vector2[] samples);

        protected DetailObjectNode GetOutputValue() {
            NodePort port = GetOutputPort("Output");
            if (port == null || port.Connection == null) {
                return null;
            }

            var val = port.Connection.node as DetailObjectNode;

            return val;
        }
    }

    [Serializable]
    public abstract class DetailObjectNode : PreviewableNode {
        public enum Distribution {
            PoissonDisc, Uniform
        }

        [Output]
        public NodePort Output;

        [Input(ShowBackingValue.Never, ConnectionType.Override)]
        public ConstraintNode Constraint;

        [Input(ShowBackingValue.Never, ConnectionType.Override)]
        public DetailModifierNode Modifier;
        
        public float BendFactor = 0f;

        public Distribution DistributionType;
        public float Spread = 5f;
        public int UniformResolution;

        public int MaxObjects = 500;

        public Vector2 WidthScale = new Vector2(1f, 1.5f);
        public Vector2 HeightScale = new Vector2(1f, 1.5f);
        
        public ConstraintNode ConstraintValue {
            get {
                return GetInputValue<ConstraintNode>("Constraint");
            }
        }

        private ConstraintNode _cachedCv = null;

        public override Texture2D DidRequestTextureUpdate() {
            Texture2D tex = new Texture2D(PreviewTextureSize, PreviewTextureSize);
            ConstraintNode cons = ConstraintValue;

            //Fill texture with black
            for (int x = 0; x < PreviewTextureSize; x++) {
                for (int y = 0; y < PreviewTextureSize; y++) {
                    tex.SetPixel(x, y, Color.black);
                }
            }

            AbsGeneratorNode maskNode = null;
            Generator gen = null;
            if (cons != null)
                maskNode = cons.GetMaskValue();
            if (maskNode != null)
                gen = maskNode.GetGenerator();

            Vector2[] samples = SamplePositions();
            for (var i = 0; i < samples.Length; i++) {
                Vector2 sample = samples[i];

                if (gen != null) {
                    Vector2 world = MathUtil.NormalToWorld(GridPosition.Zero, sample);

                    if (gen.GetValue(world.x, world.y, 0) < cons.Tolerance) {
                        continue;
                    }
                }

                int x = Mathf.Clamp((int)(sample.x * PreviewTextureSize), 0, PreviewTextureSize);
                int y = Mathf.Clamp((int)(sample.y * PreviewTextureSize), 0, PreviewTextureSize);

                tex.SetPixel(x, y, Color.white);

                if (i >= MaxObjects) {
                    break;
                }
            }
           
            tex.Apply();
            return tex;
        }

        public Vector2[] SamplePositions() {
            Vector2[] samples = null;
            switch (DistributionType) {
                case Distribution.PoissonDisc:
                    samples = GetPoissonGridSamples();
                    break;
                case Distribution.Uniform:
                    samples = GetUniformSamples();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            DetailModifierNode mod = GetInputValue<DetailModifierNode>("Modifier");
            if (mod != null) {
                return mod.SamplePositions(samples);
            }

            return samples;
        }

        /// <summary>
        /// Decides whether or not this object should be placed at
        /// the specified height and angle. Does not check for intersections.
        /// </summary>
        /// <param name="height">Height to evaluate at</param>
        /// <param name="angle">Angle to evaluate at (0 to 90 degrees)</param>
        public bool ShouldPlaceAt(float x, float y, float height, float angle) {
            if (_cachedCv == null) {
                _cachedCv = ConstraintValue;

                if (_cachedCv == null) {
                    return true;
                }
            }

            return _cachedCv.ShouldPlaceAt(x, y, height, angle);
        }

        protected float GetRandomWidthScale() {
            return UnityEngine.Random.Range(WidthScale.x, WidthScale.y);
        }

        protected float GetRandomHeight() {
            return UnityEngine.Random.Range(HeightScale.x, HeightScale.y);
        }

        /// <summary>
        /// Creates a list of positions in the range of [0, 1] by 
        /// running the poisson disc sampling algorithm.
        /// </summary>
        /// <param name="density">Density of the placement of objects</param>
        /// <param name="gridSize">Size of the grid to sample</param>
        private Vector2[] GetPoissonGridSamples(float density, int gridSize) {
            PoissonDiscSampler pds = new PoissonDiscSampler(gridSize, gridSize, density, TerraConfig.Instance.Seed);
            Boo.Lang.List<Vector2> total = new Boo.Lang.List<Vector2>();

            foreach (Vector2 sample in pds.Samples()) {
                total.Add(sample / gridSize);
            }

            return total.ToArray();
        }
        
        /// <inheritdoc cref="GetPoissonGridSamples(float,int)"/>
        private Vector2[] GetPoissonGridSamples(int gridSize = 100) {
            return GetPoissonGridSamples(Spread, gridSize);
        }

        private Vector2[] GetUniformSamples() {
            List<Vector2> samples = new List<Vector2>(UniformResolution * UniformResolution);

            for (int x = 0; x < UniformResolution; x++) {
                for (int y = 0; y < UniformResolution; y++) {
                    samples.Add(new Vector2(x / (float) UniformResolution, y / (float) UniformResolution));
                }
            }

            return samples.ToArray();
        }
    }
}