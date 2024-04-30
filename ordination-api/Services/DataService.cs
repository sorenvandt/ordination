using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using shared.Model;
using static shared.Util;
using Data;

namespace Service;

public class DataService
{
    private OrdinationContext db { get; }

    public DataService(OrdinationContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Seeder noget nyt data i databasen, hvis det er nødvendigt.
    /// </summary>
    public void SeedData()
    {

        // Patients
        Patient[] patients = new Patient[5];
        patients[0] = db.Patienter.FirstOrDefault()!;

        if (patients[0] == null)
        {
            patients[0] = new Patient("121256-0512", "Jane Jensen", 23.9);
            patients[1] = new Patient("070985-1153", "Finn Madsen", 83.2);
            patients[2] = new Patient("050972-1233", "Hans Jørgensen", 120.0);
            patients[3] = new Patient("011064-1522", "Ulla Nielsen", 59.9);
            patients[4] = new Patient("123456-1234", "Ib Hansen", 87.7);

            db.Patienter.Add(patients[0]);
            db.Patienter.Add(patients[1]);
            db.Patienter.Add(patients[2]);
            db.Patienter.Add(patients[3]);
            db.Patienter.Add(patients[4]);
            db.SaveChanges();
        }

        Laegemiddel[] laegemiddler = new Laegemiddel[5];
        laegemiddler[0] = db.Laegemiddler.FirstOrDefault()!;
        if (laegemiddler[0] == null)
        {
            laegemiddler[0] = new Laegemiddel("Acetylsalicylsyre", 0.1, 0.15, 0.16, "Styk");
            laegemiddler[1] = new Laegemiddel("Paracetamol", 1, 1.5, 2, "Ml");
            laegemiddler[2] = new Laegemiddel("Fucidin", 0.025, 0.025, 0.025, "Styk");
            laegemiddler[3] = new Laegemiddel("Methotrexat", 0.01, 0.015, 0.02, "Styk");
            laegemiddler[4] = new Laegemiddel("Prednisolon", 0.1, 0.15, 0.2, "Styk");

            db.Laegemiddler.Add(laegemiddler[0]);
            db.Laegemiddler.Add(laegemiddler[1]);
            db.Laegemiddler.Add(laegemiddler[2]);
            db.Laegemiddler.Add(laegemiddler[3]);
            db.Laegemiddler.Add(laegemiddler[4]);

            db.SaveChanges();
        }

        Ordination[] ordinationer = new Ordination[6];
        ordinationer[0] = db.Ordinationer.FirstOrDefault()!;
        if (ordinationer[0] == null)
        {
            Laegemiddel[] lm = db.Laegemiddler.ToArray();
            Patient[] p = db.Patienter.ToArray();

            ordinationer[0] = new PN(new DateTime(2024, 1, 1), new DateTime(2024, 1, 12), 123, lm[1]);
            ordinationer[1] = new PN(new DateTime(2024, 2, 12), new DateTime(2024, 2, 14), 3, lm[0]);
            ordinationer[2] = new PN(new DateTime(2024, 1, 20), new DateTime(2024, 1, 25), 5, lm[2]);
            ordinationer[3] = new PN(new DateTime(2024, 1, 1), new DateTime(2024, 1, 12), 123, lm[1]);
            ordinationer[4] = new DagligFast(new DateTime(2021, 1, 10), new DateTime(2024, 1, 12), lm[1], 2, 0, 1, 0);
            ordinationer[5] = new DagligSkæv(new DateTime(2021, 1, 23), new DateTime(2024, 1, 24), lm[2]);

            ((DagligSkæv)ordinationer[5]).doser = new Dosis[] {
                new Dosis(CreateTimeOnly(12, 0, 0), 0.5),
                new Dosis(CreateTimeOnly(12, 40, 0), 1),
                new Dosis(CreateTimeOnly(16, 0, 0), 2.5),
                new Dosis(CreateTimeOnly(18, 45, 0), 3)
            }.ToList();


            db.Ordinationer.Add(ordinationer[0]);
            db.Ordinationer.Add(ordinationer[1]);
            db.Ordinationer.Add(ordinationer[2]);
            db.Ordinationer.Add(ordinationer[3]);
            db.Ordinationer.Add(ordinationer[4]);
            db.Ordinationer.Add(ordinationer[5]);

            db.SaveChanges();

            p[0].ordinationer.Add(ordinationer[0]);
            p[0].ordinationer.Add(ordinationer[1]);
            p[2].ordinationer.Add(ordinationer[2]);
            p[3].ordinationer.Add(ordinationer[3]);
            p[1].ordinationer.Add(ordinationer[4]);
            p[1].ordinationer.Add(ordinationer[5]);

            db.SaveChanges();
        }
    }


    public List<PN> GetPNs()
    {
        return db.PNs.Include(o => o.laegemiddel).Include(o => o.dates).ToList();
    }

    public List<DagligFast> GetDagligFaste()
    {
        return db.DagligFaste
            .Include(o => o.laegemiddel)
            .Include(o => o.MorgenDosis)
            .Include(o => o.MiddagDosis)
            .Include(o => o.AftenDosis)
            .Include(o => o.NatDosis)
            .ToList();
    }

    public List<DagligSkæv> GetDagligSkæve()
    {
        return db.DagligSkæve
            .Include(o => o.laegemiddel)
            .Include(o => o.doser)
            .ToList();
    }

    public List<Patient> GetPatienter()
    {
        return db.Patienter.Include(p => p.ordinationer).ToList();
    }

    public List<Laegemiddel> GetLaegemidler()
    {
        return db.Laegemiddler.ToList();
    }

    public PN OpretPN(int patientId, int laegemiddelId, double antal, DateTime startDato, DateTime slutDato)
    {
        Patient patient = db.Patienter.Find(patientId);
        Laegemiddel lm = db.Laegemiddler.Find(laegemiddelId);
        PN newPN = new PN(startDato, slutDato, antal, lm);
        db.PNs.Add(newPN);
        patient.ordinationer.Add(newPN);
        db.SaveChanges();

        return newPN;
    }

    public DagligFast OpretDagligFast(int patientId, int laegemiddelId,
        double antalMorgen, double antalMiddag, double antalAften, double antalNat,
        DateTime startDato, DateTime slutDato)
    {
        int negativeCount = 0;
        int zeroCount = 0;

        if (antalMorgen == 0) zeroCount++;
        else if (antalMorgen < 0) negativeCount++;

        if (antalMiddag == 0) zeroCount++;
        else if (antalMiddag < 0) negativeCount++;

        if (antalAften == 0) zeroCount++;
        else if (antalAften < 0) negativeCount++;

        if (antalNat == 0) zeroCount++;
        else if (antalNat < 0) negativeCount++;

        if (zeroCount > 3 || negativeCount > 0)
        {
            throw new ArgumentException("Antallet af morgen, middag, aften og nat skal være positive tal, og der må maksimalt være 2 gange 0 doser.");
        }

        if (!double.TryParse(antalMorgen.ToString(), out _) ||
            !double.TryParse(antalMiddag.ToString(), out _) ||
            !double.TryParse(antalAften.ToString(), out _) ||
            !double.TryParse(antalNat.ToString(), out _))
        {
            throw new ArgumentException("Antallet af morgen, middag, aften og nat skal være numeriske værdier.");
        }

        Patient patient = db.Patienter.FirstOrDefault(a => a.PatientId == patientId);
        Laegemiddel lm = db.Laegemiddler.Find(laegemiddelId);
        DagligFast newDagligFast = new DagligFast(startDato, slutDato, lm, antalMorgen, antalMiddag, antalAften, antalNat);
        db.DagligFaste.Add(newDagligFast);
        patient.ordinationer.Add(newDagligFast);
        db.SaveChanges();

        return newDagligFast;
    }

    public DagligSkæv OpretDagligSkaev(int patientId, int laegemiddelId, Dosis[] doser, DateTime startDato, DateTime slutDato)
    {
        Patient patient = db.Patienter.Find(patientId);
        Laegemiddel lm = db.Laegemiddler.Find(laegemiddelId);

        if (patient == null)
        {
            throw new ArgumentNullException();
        }

        DagligSkæv newDagligSkæv = new DagligSkæv(startDato, slutDato, lm, doser);
        db.DagligSkæve.Add(newDagligSkæv);
        patient.ordinationer.Add(newDagligSkæv);
        db.SaveChanges();

        return newDagligSkæv;
    }

    public string AnvendOrdination(int id, Dato dato)
    {
        // TODO: DONE
        var ordination = db.Ordinationer.Find(id);
        if (ordination == null)
        {
            return "Ordination not found!";
        }
        db.SaveChanges();

        return "Success!";
    }

    /// <summary>
    /// Den anbefalede dosis for den pågældende patient, per døgn, hvor der skal tages hensyn til
    /// patientens vægt. Enheden afhænger af lægemidlet. Patient og lægemiddel må ikke være null.
    /// </summary>
    /// <param name="patient"></param>
    /// <param name="laegemiddel"></param>
    /// <returns></returns>
    public double GetAnbefaletDosisPerDøgn(int patientId, int laegemiddelId)
    {
        // TODO: DONE!
        //igen starter med at finde patienten og og lægemidet i databasen
        var patient = db.Patienter.Find(patientId);
        var lægemiddel = db.Laegemiddler.Find(laegemiddelId);

        //Tjek om ptient og lægemidet findes
        if (patient == null || lægemiddel == null)
        {
            //returnere en fejlkode eller kast en exception
            return -1;
        }

        //Beregner den anbefalede dosis per døgn baseret på patientens vægt og lægemidlets doseringsinformation
        double anbefaletDosisPerDøgn = 0;
        
        if (patient.vaegt < 25)
        {
            anbefaletDosisPerDøgn = lægemiddel.enhedPrKgPrDoegnLet * patient.vaegt;
        }
        else if (patient.vaegt <= 120)
        {
            anbefaletDosisPerDøgn = lægemiddel.enhedPrKgPrDoegnNormal * patient.vaegt;
        }
        else
        {
            anbefaletDosisPerDøgn = lægemiddel.enhedPrKgPrDoegnTung * patient.vaegt;
        }
        //returnere den anbefalet dosis per døgn
        return anbefaletDosisPerDøgn;
    }
}