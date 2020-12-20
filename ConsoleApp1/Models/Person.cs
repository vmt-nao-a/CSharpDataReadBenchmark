using Bogus;
using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    [Table(Name = "t_person")]
    [MessagePack.MessagePackObject]
    public class Person
    {
        /// <summary>  Setting this as a starting value for id</summary>
        private static int _userId = 100;

        [Column(Name = "id", DbType = "INT", CanBeNull = false, IsPrimaryKey = true)]
        [MessagePack.Key(0)]
        public int Id { get; set; }

        [Column(Name = "first_name", DbType = "NVARCHAR", CanBeNull = false)]
        [MessagePack.Key(1)]
        public string FirstName { get; set; }

        [Column(Name = "middle_name", DbType = "NVARCHAR", CanBeNull = true)]
        [MessagePack.Key(2)]
        public string MiddleName { get; set; }

        [Column(Name = "last_name", DbType = "NVARCHAR", CanBeNull = false)]
        [MessagePack.Key(3)]
        public string LastName { get; set; }

        [Column(Name = "title", DbType = "NVARCHAR", CanBeNull = true)]
        [MessagePack.Key(4)]
        public string Title { get; set; }

        [Column(Name = "dob", DbType = "NVARCHAR", CanBeNull = true)]
        [MessagePack.Key(5)]
        public DateTime DOB { get; set; }

        [Column(Name = "email", DbType = "NVARCHAR", CanBeNull = true)]
        [MessagePack.Key(6)]
        public string Email { get; set; }

        [Column(Name = "gender", DbType = "INT", CanBeNull = true)]
        [MessagePack.Key(7)]
        public Gender Gender { get; set; }

        [Column(Name = "ssn", DbType = "NVARCHAR", CanBeNull = true)]
        [MessagePack.Key(8)]
        public string SSN { get; set; }

        [Column(Name = "suffix", DbType = "NVARCHAR", CanBeNull = true)]
        [MessagePack.Key(9)]
        public string Suffix { get; set; }

        [Column(Name = "phone", DbType = "NVARCHAR", CanBeNull = true)]
        [MessagePack.Key(10)]
        public string Phone { get; set; }

        /// <summary>Gets the fake data.</summary>
        /// <value>The fake data.</value>
        public static Faker<Person> FakeData { get; } =
            new Faker<Person>()
                .RuleFor(p => p.Id, f => _userId++)
                .RuleFor(p => p.FirstName, f => f.Name.FirstName())
                .RuleFor(p => p.MiddleName, f => f.Name.FirstName())
                .RuleFor(p => p.LastName, f => f.Name.LastName())
                .RuleFor(p => p.Title, f => f.Name.Prefix(f.Person.Gender))
                .RuleFor(p => p.Suffix, f => f.Name.Suffix())
                .RuleFor(p => p.Email, (f, p) => f.Internet.Email(p.FirstName, p.LastName))
                .RuleFor(p => p.DOB, f => f.Date.Past(18))
                .RuleFor(p => p.Gender, f => f.PickRandom<Gender>())
                .RuleFor(p => p.SSN, f => f.Random.Replace("###-##-####"))
                .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber("(###)-###-####"));

    }
    public enum Gender : int
    {
        None = 0,
        Male,
        Female,
        Other
    }
}
