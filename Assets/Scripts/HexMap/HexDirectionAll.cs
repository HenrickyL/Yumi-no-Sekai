using System.Collections.Generic;
using UnityEngine;

public enum HexDirectionAll {
	N, NE, E, SE, S, SW, W, NW
}
public static class HexDirectionAllExtensions {
	public static float ToDegrees (this HexDirectionAll direction) {
		return ((int)direction)*45f;
	}
	public static HexDirectionAll ToDirection (this float degrees) {
		return (HexDirectionAll)(degrees/45f);
	}
    public static HexDirectionAll HalfCircleToDirection (this float degrees) {
        degrees = degrees + 180f;
		return (HexDirectionAll)(degrees/45f);
	}
    public static bool IsFront (this HexDirectionAll direction) {
		return new List<HexDirectionAll>(){HexDirectionAll.NE, HexDirectionAll.N,HexDirectionAll.NW}.Contains(direction);
	}
    public static bool IsBack (this HexDirectionAll direction) {
		return new List<HexDirectionAll>(){HexDirectionAll.SE, HexDirectionAll.S,HexDirectionAll.SW}.Contains(direction);
	}
    public static bool IsRight (this HexDirectionAll direction) {
		return new List<HexDirectionAll>(){HexDirectionAll.NE, HexDirectionAll.E,HexDirectionAll.SE}.Contains(direction);
	}

    public static bool IsLeft (this HexDirectionAll direction) {
		return new List<HexDirectionAll>(){HexDirectionAll.NW, HexDirectionAll.W,HexDirectionAll.SW}.Contains(direction);
	}
    public static HexDirectionAll Mirror (this HexDirectionAll direction) {
		return (HexDirectionAll)(((int)direction+4)%8);
	}

}