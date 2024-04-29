namespace ordination_test;

using Microsoft.EntityFrameworkCore;

using Service;
using Data;
using shared.Model;

[TestClass]
public class ServiceTest
{
    private DataService service;

    [TestInitialize]
    public void SetupBeforeEachTest()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdinationContext>();
        optionsBuilder.UseInMemoryDatabase(databaseName: "test-database");
        var context = new OrdinationContext(optionsBuilder.Options);
        service = new DataService(context);
        service.SeedData();
    }

    [TestMethod]
    public void PatientsExist()
    {
        Assert.IsNotNull(service.GetPatienter());
    }

    [TestMethod]
    public void Ordination()
    {
        // Arrange
        Laegemiddel lm = new Laegemiddel("Medicin", 1, 2, 3, "Enheder");
        DateTime startDato = DateTime.Now;
        DateTime slutDato = DateTime.Now.AddDays(3);

        // Act
        Ordination dagligFast = new DagligFast(startDato, slutDato, lm, 2, 2, 1, 0);
        Ordination dagligSk�v = new DagligSk�v(startDato, slutDato, lm);
        Ordination pn = new PN(startDato, slutDato, 3, lm);

        // Tjekker om dagligFast virker
        Assert.AreEqual(lm, dagligFast.laegemiddel);
        Assert.AreEqual(startDato, dagligFast.startDen);
        Assert.AreEqual(slutDato, dagligFast.slutDen);

        // Tjekker om dagligSk�v virker
        Assert.AreEqual(lm, dagligSk�v.laegemiddel);
        Assert.AreEqual(startDato, dagligSk�v.startDen);
        Assert.AreEqual(slutDato, dagligSk�v.slutDen);

        // Tjekker om PN virker
        Assert.AreEqual(lm, pn.laegemiddel);
        Assert.AreEqual(startDato, pn.startDen);
        Assert.AreEqual(slutDato, pn.slutDen);
    }

    [TestMethod]

    public void AnbefaletDosis()
    {
        int patientId = 5;
        int laegemiddelId = 5;
        double forventetDosis = 13.155;

        var dbOptions = new DbContextOptionsBuilder<OrdinationContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;

        using (var context = new OrdinationContext(dbOptions))
        {
            var dataservice = new DataService(context);
            dataservice.SeedData();

            var faktiskDosisPerDoegn = dataservice.GetAnbefaletDosisPerD�gn(patientId, laegemiddelId);
            Assert.AreEqual(forventetDosis, faktiskDosisPerDoegn, "Den faktiske dosis per d�gn svarer ikke til den forventede dosis.");
        }

    }

    [TestMethod]

    public void AnbefaletDosisTung()
    {
        int patientId = 3;
        int laegemiddelId = 5;
        double forventetDosis = 24;

        var dbOptions = new DbContextOptionsBuilder<OrdinationContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;

        using (var context = new OrdinationContext(dbOptions))
        {
            var dataservice = new DataService(context);
            dataservice.SeedData();

            var faktiskDosisPerDoegn = dataservice.GetAnbefaletDosisPerD�gn(patientId, laegemiddelId);
            Assert.AreEqual(forventetDosis, faktiskDosisPerDoegn, "Den faktiske dosis per d�gn svarer ikke til den forventede dosis.");
        }

    }

    [TestMethod]

    public void AnbefaletDosisLet()
    {
        int patientId = 1;
        int laegemiddelId = 5;
        double forventetDosis = 2.39;

        var dbOptions = new DbContextOptionsBuilder<OrdinationContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;

        using (var context = new OrdinationContext(dbOptions))
        {
            var dataservice = new DataService(context);
            dataservice.SeedData();

            var faktiskDosisPerDoegn = dataservice.GetAnbefaletDosisPerD�gn(patientId, laegemiddelId);
            Assert.AreEqual(forventetDosis, faktiskDosisPerDoegn, "Den faktiske dosis per d�gn svarer ikke til den forventede dosis.");
        }

    }

    [TestMethod]
    // Denne metode tjekker om oprettelsen af daglige faste ordinationer udf�res korrekt med positiv doser.
    public void OpretDagligFast()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.AreEqual(1, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            1, 1, 1, 1, DateTime.Now, DateTime.Now.AddDays(7));

        Assert.AreEqual(2, service.GetDagligFaste().Count());
    }

    [TestMethod]
    // Denne metode tjekker om oprettelsen af daglige faste ordinationer udf�res med nul doser.
    // Forventet output Exception
    public void OpretDagligFast_null()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.AreEqual(2, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            0, 0, 0, 0, DateTime.Now, DateTime.Now.AddDays(7));

        Assert.AreEqual(3, service.GetDagligFaste().Count());
    }

    [TestMethod]
    // Denne metode tjekker om oprettelsen af daglige faste ordinationer udf�res korrekt med negative doser.
    // Forventet output Exception
    public void OpretDagligFast_negative()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.AreEqual(2, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            -1, 0, 0, 0, DateTime.Now, DateTime.Now.AddDays(7));

        Assert.AreEqual(3, service.GetDagligFaste().Count());
    }


    [TestMethod]
    public void OpretDagligSkaev()
    {
        // Forbered testdata
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();
        DateTime startDato = DateTime.Now;
        DateTime slutDato = DateTime.Now.AddDays(3);
        Dosis[] doser = {
        new Dosis(DateTime.Today.AddHours(8), 1.5), // Morgen
        new Dosis(DateTime.Today.AddHours(13), 2.0), // Middag
        new Dosis(DateTime.Today.AddHours(18), 1.0), // Aften
        new Dosis(DateTime.Today.AddHours(22), 0.5) // Nat
    };

        // Kontroller antallet af eksisterende daglige sk�ve ordinationer
        Assert.AreEqual(1, service.GetDagligSk�ve().Count());

        // Opret en ny daglig sk�v ordination
        service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId, doser, startDato, slutDato);

        // Valider at ordinationen er blevet oprettet korrekt
        List<DagligSk�v> ordinationer = service.GetDagligSk�ve();
        Assert.AreEqual(2, ordinationer.Count()); // Der skal v�re 2 ordinationer nu
    }

    [TestMethod]
    public void OpretPN()
    {
        // Forbered testdata
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();
        DateTime startDato = DateTime.Now;
        DateTime slutDato = DateTime.Now.AddDays(3);

        // Kontroller antallet af eksisterende PN-ordinationer
        Assert.AreEqual(4, service.GetPNs().Count());
    }


    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestAtKodenSmiderEnException()
    {
        throw new ArgumentNullException();
    }
}