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
            //version of saveSystem, update each time it is changed
            writer.Write((byte) 2);
            Vector3 v3 = _player.transform.position;
            writer.Write((int) (v3.x * 10f));
            writer.Write((int) (v3.y * 10f));
            writer.Write((int) (v3.z * 10f));
            Debug.Log(v3);
            writer.Write((int) (_vCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value * 10f));
            writer.Write((int) (_vCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value * 10f));
            v3 = _pCamera.transform.position;
            writer.Write((int) (v3.x * 10f));
            writer.Write((int) (v3.y * 10f));
            writer.Write((int) (v3.z * 10f));
            v3 = _pCamera.transform.rotation.eulerAngles;
            writer.Write((int) (v3.x * 10f));
            writer.Write((int) (v3.y * 10f));
            writer.Write((int) (v3.z * 10f));
            writer.Write((byte) _pCamera.Cam.fieldOfView);
        }
    }

    public void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, "save.me");
        using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
        {
            if (reader.ReadByte() == 2)
            {
                _player.transform.position = Vector3.right * reader.ReadInt32() / 10f +
                                             Vector3.up * reader.ReadInt32() / 10f +
                                             Vector3.forward * reader.ReadInt32() / 10f;
                
                Debug.Log(_player.transform.position);
                _vCamera.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value = reader.ReadInt32() / 10f;
                _vCamera.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value = reader.ReadInt32() / 10f;
                _pCamera.transform.position = Vector3.right * reader.ReadInt32() / 10f +
                                              Vector3.up * reader.ReadInt32() / 10f +
                                              Vector3.forward * reader.ReadInt32() / 10f;
                _pCamera.transform.rotation = Quaternion.Euler(Vector3.right * reader.ReadInt32() / 10f +
                                                               Vector3.up * reader.ReadInt32() / 10f +
                                                               Vector3.forward * reader.ReadInt32() / 10f);
                _pCamera.Cam.fieldOfView = reader.ReadByte();
            }
        }

        _pCamera.AdjustZoom(0);
        //todo make player drop if they're holding the camera
    }
}