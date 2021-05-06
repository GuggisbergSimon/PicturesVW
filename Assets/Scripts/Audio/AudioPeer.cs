using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPeer : MonoBehaviour
{
    [SerializeField] private float bufferInitialDecay = 0.005f, bufferMultiplierDecay = 1.2f;
    private AudioSource _audioSource;
    public static float[] samples = new float[512];
    private float[] _frequBand = new float[8];
    private float[] _bandBuffer = new float[8];
    private float[] _bufferDecrease = new float[8];
    private float[] _freqBandHighest = new float[8];
    
    public static float[] audioBand = new float[8];
    public static float[] audioBandBuffer = new float[8];
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        GetSpectrumAudioSource();
        MakeFrequencyBands();
        BandBuffer();
        CreateAudioBands();
    }

    private void CreateAudioBands()
    {
        for (int i = 0; i < 8; i++)
        {
            if (_frequBand[i] > _freqBandHighest[i])
            {
                _freqBandHighest[i] = _frequBand[i];
            }

            audioBand[i] = _frequBand[i] / _freqBandHighest[i];
            audioBandBuffer[i] = _bandBuffer[i] / _freqBandHighest[i];
        }
    }

    private void BandBuffer()
    {
        for (int i = 0; i < 8; i++)
        {
            if (_frequBand[i] > _bandBuffer[i])
            {
                _bandBuffer[i] = _frequBand[i];
                _bufferDecrease[i] = bufferInitialDecay;
            }

            if (_frequBand[i] < _bandBuffer[i])
            {
                _bandBuffer[i] -= _bufferDecrease[i];
                _bufferDecrease[i] *= bufferMultiplierDecay;
            }
        }
    }

    private void GetSpectrumAudioSource()
    {
        _audioSource.GetSpectrumData(samples, 0, FFTWindow.Blackman);
    }

    private void MakeFrequencyBands()
    {
        int count = 0;
        for (int i = 0; i < _frequBand.Length; i++)
        {
            float average = 0f;
            int sampleCount = (int) Mathf.Pow(2, i) * 2;

            if (i + 1 == _frequBand.Length)
            {
                sampleCount += 2;
            }

            for (int j = 0; j < sampleCount; j++)
            {
                average += samples[count] * (count + 1);
                count++;
            }

            average /= count;
            _frequBand[i] = average * 10;
        }
    }
}