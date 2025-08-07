using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class VM_Soal
{
    private List<M_Soal> currSoal = new List<M_Soal>();
    public List<M_Soal> CurrSoal { get => currSoal; set => currSoal = value; }    

    private int currIndex;
    public int CurrIndex { get => currIndex; set => currIndex = value; }

    private int banyakPilihanJawaban;
    public int BanyakPilihanJawaban { get => banyakPilihanJawaban; set => banyakPilihanJawaban = value; }   

    public VM_Soal(int _opsiJawaban)
    {
        // curr index
        currIndex = 0;

        // banyak pilihan jawaban
        banyakPilihanJawaban = _opsiJawaban;

        // ambil soal
        currSoal = ParsingSoalJawaban();
    }

    public List<M_Soal> ParsingSoalJawaban()
    {
        List<M_Soal> soalList = new List<M_Soal>();
        string bankSoal = BankSoal.banksoaljawaban;
        string[] bagiPerSoal = bankSoal.Split('#');

        foreach (var soal in bagiPerSoal)
        {
            if (string.IsNullOrWhiteSpace(soal)) continue;

            string[] bagiPerBintang = soal.Split('*');

            // Validasi jumlah elemen: soal + pilihan + kunci
            int expectedLength = banyakPilihanJawaban + 2;
            if (bagiPerBintang.Length < expectedLength)
            {
                Debug.LogError($"Format soal tidak valid. Ditemukan {bagiPerBintang.Length}, seharusnya minimal {expectedLength}. Data: {soal}");
                continue;
            }

            List<string> _pilihanJawabans = new List<string>();

            for (int i = 1; i <= banyakPilihanJawaban; i++)
            {
                _pilihanJawabans.Add(bagiPerBintang[i]);
            }

            int _kunci = -1;
            if (!int.TryParse(bagiPerBintang[expectedLength - 1], out _kunci))
            {
                Debug.LogError($"Index jawaban tidak valid: {bagiPerBintang[expectedLength - 1]} dalam soal: {soal}");
                continue;
            }

            if (_kunci < 0 || _kunci >= banyakPilihanJawaban)
            {
                Debug.LogError($"Index jawaban di luar batas: {soal}");
                continue;
            }

            M_Soal _soal = new M_Soal()
            {
                soal = bagiPerBintang[0],
                kunci = _kunci,
                pilihanJawabans = _pilihanJawabans,
                isEnd = false,
                idx = soalList.Count
            };

            soalList.Add(_soal);
        }

        return soalList;
    }


    public M_Soal PilihSoalAvailable()
    {
        // ambil soal yang tersedia (isEnd = false)
        currSoal = currSoal.Where(x => x.isEnd.Equals(false)).ToList();        
        currIndex = Random.Range(0, currSoal.Count);
        //
        return currSoal[currIndex];
    }

    public bool IsMenjawab(int _idPilih, int idKunci)
    {
        bool isbenar = false;

        // validasi jawaban benar-salah
        if (_idPilih == idKunci)
            isbenar = true;

        return isbenar;
    }

    public bool CekSoalSudahSemua()
    {
        return currSoal.Where(x => x.isEnd.Equals(false)).Count() == 0 ? false : true;
    }

    public void CekKetersediaanSoalDebugging()
    {
        // 
        Debug.Log("soal tersedia");
        foreach (var item in CurrSoal)
        {
            Debug.Log("soal: " + item.soal + ", tersedia: " + item.isEnd);
        }
        Debug.Log("-------");
    }

    
}