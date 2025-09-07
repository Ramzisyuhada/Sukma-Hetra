// pada hierarki, V_Soal diattach pada gameobject
// kemudian harus ada UI untuk text soal
// arahkan gameobject UI text tersebut ke variable tampilSoal

// untuk tombol jawaban benar/tidak:
// buat 2 tombol untuk jawaban benar dan tidak, silahkan ambil dari prefabs
// teks tombol 1 diisikan tulisan Benar (berhubungan dengan fungsi IsMenjawab)
// teks tombol 1 diisikan tulisan Salah (berhubungan dengan fungsi IsMenjawab)
// Pada event On Click dari masing-masing tombol, diisikan dari V_Soal.CekJawaban

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class V_Soal : MonoBehaviour
{
    // Wajib ada Gameobject UI TextMeshProUGUI
    // untuk menampilkan soal, 
    // referensikan ke inspektor/panggil via script
    // gameobject textmeshpro untuk tampilan soal
    [SerializeField] TextMeshProUGUI tampilSoal;

    // untuk menampilkan akurasi jawaban benar
    [SerializeField] TextMeshProUGUI akurasiTMP;

    // parent untuk pilihanjawaban
    [SerializeField] GameObject parentPilihanJawaban;    

    // varaible penghimpun soal-soal dan jawaban serta idnex jawaban benar
    [SerializeField] List<M_Soal> soalsoal = new List<M_Soal>();

    [SerializeField] M_Soal soalTerpilih;
    VM_Soal vm_soal;

    int banyakJawabanTersedia;

    [SerializeField] float jawabanBenar = 0;
    [SerializeField] float soalCurr = 0;


    [Header("Controller")]
    [SerializeField] private InputActionProperty inputActionLeft;
    [SerializeField] private InputActionProperty inputActionRight;
    [SerializeField] private XRRayInteractor rayLeft;
    [SerializeField] private XRRayInteractor rayRight;


    public GameObject gambar19;
    public GameObject gambar20;

    private GameObject g;

    private XRController controller;
    private static float nilai ;
    // Start is called before the first frame update


    public void close()
    {
        GameObject obj = GameObject.Find("Pertanyaan(Clone)");
        Destroy(obj != null ? obj : null);
    }
    void Start()
    {
        

        if(nilai != null)
        {
            akurasiTMP.text = nilai.ToString("#.##") + "%";

        }




        if (parentPilihanJawaban.transform.childCount == 0) return;
        
        banyakJawabanTersedia = parentPilihanJawaban.transform.childCount;
        InitSoal();
    }

    void InitSoal()
    {
        // bikin objek
        vm_soal = new VM_Soal(banyakJawabanTersedia);
        // ambil soal yang ada
        soalsoal = vm_soal.CurrSoal;

        TampilkanSoalJawaban();
    }    

    void TampilkanSoalJawaban()
    {
        // ambil soal yang terpilih
        soalTerpilih = vm_soal.PilihSoalAvailable();
        // tempelin soal ke text
        tampilSoal.text = soalTerpilih.soal;        
        // generate pilihan jawaban
        GeneratePilihanJawaban();

        if (soalsoal.IndexOf(soalTerpilih) == 19)
        {
            gambar19.SetActive(true);
            gambar20.SetActive(false);
        }
        else if (soalsoal.IndexOf(soalTerpilih) == 20)
        {
            gambar19.SetActive(false);
            gambar20.SetActive(true);
        }
        else
        {
            gambar19.SetActive(false);
            gambar20.SetActive(false);
        }
    }

    void GeneratePilihanJawaban()
    {
        int idx = 0;
        foreach (Transform item in parentPilihanJawaban.transform)
        {
            item.gameObject.AddComponent(typeof(V_Temp));
            item.GetComponent<V_Temp>()._index = idx;
            item.GetComponent<V_Temp>()._isEnd = false;

            item.GetComponent<Image>().color = Color.white;
            item.GetComponent<Button>().onClick.AddListener(CekJawaban);
            item.GetComponentInChildren<TextMeshProUGUI>().text = "\t" +
                soalTerpilih.pilihanJawabans[idx];

            idx++;
        }
    }

    
    public void CekJawaban()
    {
        //if( vm_soal.IsMenjawab(UnityEngine.EventSystems.EventSystem
        //    .current.currentSelectedGameObject))
        //{
        //    Debug.Log("Jawaban Benar");            
        //}
        //else
        //{
        //    Debug.Log("Jawaban Salah");
        //}

        // get GO UI


         g = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        // cek jawaban
        bool cek = vm_soal.IsMenjawab(g.GetComponent<V_Temp>()._index, soalTerpilih.kunci);

        // set warna button terpilih
        SetWarnaButton(g, cek);

        // kumulasi jawaban benar
        if (cek) jawabanBenar++;

        // kumulasi pertanyaan
        soalCurr++;

        // remove listener
        foreach (Transform item in parentPilihanJawaban.transform)
            item.GetComponent<Button>().onClick.RemoveAllListeners();

        StartCoroutine(Hold());
    }

    void SetWarnaButton(GameObject _btn, bool _hasiljawab)
    {
        _btn.GetComponent<Image>().color = 
            !_hasiljawab ? Color.red : Color.green;
    }

    float AkurasiJawabanBenar()
    {
        nilai = (jawabanBenar / soalCurr) * 100;
        return (jawabanBenar / soalCurr) * 100;
    }

    IEnumerator Hold()
    {
        yield return new WaitForSeconds(1.5f);

        // akurasi jawaban user
        akurasiTMP.text = AkurasiJawabanBenar().ToString("#.##") + "%";

        TampilkanSoalJawaban();
    }
}