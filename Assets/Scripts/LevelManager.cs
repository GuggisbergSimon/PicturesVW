using System.IO;
using Cinemachine;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private PlayerController _player;
    public PlayerController Player => _player;
    private CinemachineVirtualCamera _vCamera;
    public CinemachineVirtualCamera VCamera => _vCamera;
    private PortableCamera _pCamera;
    public PortableCamera PCamera => _pCamera;
    

    private void Start()
    {
        _player = FindObjectOfType<PlayerController>();
        _vCamera = FindObjectOfType<CinemachineVirtualCamera>();
        _pCamera = FindObjectOfType<PortableCamera>();
    }

    //code taken from Catlikecoding : https://catlikecoding.com/unity/tutorials/hex-map/part-12/
    public void Save()
    {
        //note we save data very precisely so might be better to reduce size by using int
        string path = Path.Combine(Application.persistentDataPath, "save.me");
        using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            Vector3 v3 = _player.transform.position;
            writer.Write((double) v3.x);
            writer.Write((double) v3.y);
            writer.Write((double) v3.z);
            writer.Write((double) _vCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value);
            writer.Write((double) _vCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value);
            v3 = _pCamera.transform.position;
            writer.Write((double) v3.x);
            writer.Write((double) v3.y);
            writer.Write((double) v3.z);
            v3 = _pCamera.transform.rotation.eulerAngles;
            writer.Write((double) v3.x);
            writer.Write((double) v3.y);
            writer.Write((double) v3.z);
            writer.Write((byte)_pCamera.Cam.fieldOfView);
        }
    }

    public void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, "save.me");
        using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
        {
            _player.transform.position = Vector3.right * (float) reader.ReadDouble() +
                                        Vector3.up * (float) reader.ReadDouble() +
                                        Vector3.forward * (float) reader.ReadDouble();
            _vCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value = (float) reader.ReadDouble();
            _vCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value = (float) reader.ReadDouble();
            _pCamera.transform.position = Vector3.right * (float) reader.ReadDouble() +
                                         Vector3.up * (float) reader.ReadDouble() +
                                         Vector3.forward * (float) reader.ReadDouble();
            _pCamera.transform.rotation = Quaternion.Euler(Vector3.right * (float) reader.ReadDouble() +
                                          Vector3.up * (float) reader.ReadDouble() +
                                          Vector3.forward * (float) reader.ReadDouble());
            _pCamera.Cam.fieldOfView = reader.ReadByte();
        }
        _pCamera.RefreshZoom();
    }
}
