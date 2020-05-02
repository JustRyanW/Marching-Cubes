using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFilter {
    Noise noise = new Noise();

    public float Evaluate(Vector3 point) {
        float noiseValue = Mathf.Clamp01(noise.Evaluate(point * 0.015f));
        return noiseValue;
    }
}
