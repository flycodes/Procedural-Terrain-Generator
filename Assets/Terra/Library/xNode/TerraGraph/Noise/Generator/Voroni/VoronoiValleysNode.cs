﻿using Terra.CoherentNoise;
using Terra.CoherentNoise.Generation.Voronoi;
using Terra.Structure;
using Terra.Terrain;

namespace Terra.Nodes.Generation {
	[CreateNodeMenu(MENU_PARENT_NAME + "Valleys")]
	public class VoronoiValleysNode: AbsVoronoiNoiseNode {
		public override Generator GetGenerator() {
			VoronoiValleys2D noise = new VoronoiValleys2D(TerraConfig.GenerationSeed);
			noise.Frequency = Frequency;
			noise.Period = (int)Period;

			return noise;
		}

		public override string GetTitle() {
			return "Voronoi Valleys";
		}
	}
}
