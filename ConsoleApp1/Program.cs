using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ConsoleApp1.Models;
using MessagePack;
using Microsoft.Data.Sqlite;
using System.Data.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome Benchmark!");

            const string testFileNameJson = @".\testdata.json";
            const string testFileNameMP =  @".\testdata.msg";
            const string testFileNameSQLite =  @".\testdata.sqlite";

            if (!File.Exists(testFileNameJson) || !File.Exists(testFileNameMP) || !File.Exists(testFileNameSQLite))
            {
                Console.WriteLine("begin migration");

                // create 0.1 million test data
                var testData = Person.FakeData.Generate(100000).ToList();

                if (!File.Exists(testFileNameJson))
                {
                    Console.WriteLine("write to json");
                    var json = Utf8Json.JsonSerializer.Serialize<IEnumerable<Person>>(testData);
                    using (var writer = new FileStream(testFileNameJson, FileMode.CreateNew))
                    {
                        writer.Write(json);
                    }
                }

                if (!File.Exists(testFileNameMP))
                {
                    Console.WriteLine("write to MessagePack");
                    var msg = MessagePackSerializer.Serialize<IEnumerable<Person>>(testData);
                    using (var writer = new FileStream(testFileNameMP, FileMode.CreateNew))
                    {
                        writer.Write(msg);
                    }
                }
                if (!File.Exists(testFileNameSQLite))
                {
                    Console.WriteLine("insert to SQLite");
                    SqliteConnectionStringBuilder builder = new() { DataSource = testFileNameSQLite };
                    using (var connection = new SqliteConnection(builder.ToString()))
                    {
                        connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = @"
                                CREATE TABLE IF NOT EXISTS t_person (
                                    Id INTEGER NOT NULL PRIMARY KEY,
                                    first_name TEXT NOT NULL,
                                    middle_name TEXT,
                                    last_name TEXT NOT NULL,
                                    title TEXT,
                                    dob TEXT,
                                    email TEXT,
                                    gender INTEGER,
                                    ssn TEXT,
                                    suffix TEXT,
                                    phone TEXT
                            );
                            ";

                            cmd.ExecuteNonQuery();
                        }

                        using (DataContext context = new(connection))
                        {
                            var table = context.GetTable<Person>();
                            foreach (var t in testData) {
                                table.InsertOnSubmit(t);
                            }
                            context.SubmitChanges();
                        }
                    }
                }

                Console.WriteLine("end migration");
            }

            Console.WriteLine("begin measure for read from json file (between read file to stdout from entity)");
            var beginReadFromJsonFileDateTime = DateTime.Now;

            using (var stream = new StreamReader(testFileNameJson))
            {
                var persons = Utf8Json.JsonSerializer.Deserialize<IEnumerable<Person>>(stream.BaseStream);
                foreach (var p in persons)
                {
                    Console.WriteLine(p.ToString());
                }
            }

            var endReadFromJsonFileDateTime = DateTime.Now;
            Console.WriteLine("end measure for read from json file (between read file to stdout from entity)");

            Console.WriteLine("begin measure for read from Message-Pack file (between read file to stdout from entity)");
            var beginReadFromMessagePackFileDateTime = DateTime.Now;

            using (var stream = new StreamReader(testFileNameMP))
            {
                var persons = MessagePackSerializer.Deserialize<IEnumerable<Person>>(stream.BaseStream);
                foreach (var p in persons)
                {
                    Console.WriteLine(p.ToString());
                }
            }
            var endReadFromMessagePackFileDateTime = DateTime.Now;
            Console.WriteLine("end measure for read from Message-Pack file (between read file to stdout from entity)");

            Console.WriteLine("begin measure for read from SQLite (between read file to stdout from entity)");
            var beginReadFromSQLiteFileDateTime = DateTime.Now;

            SqliteConnectionStringBuilder sqliteConnectionStringBuilder = new() { DataSource = testFileNameSQLite };
            using (var connection = new SqliteConnection(sqliteConnectionStringBuilder.ToString()))
            {
                connection.Open();
                using (DataContext context = new(connection))
                {
                    var table = context.GetTable<Person>();
                    foreach (var t in table) {
                        Console.WriteLine(t.ToString());
                    }
                }
            }
            var endReadFromSQLiteFileDateTime = DateTime.Now;
            Console.WriteLine("end measure for read from SQLite (between read file to stdout from entity)");

            var diffReadFromJsonFileDateTime = endReadFromJsonFileDateTime - beginReadFromJsonFileDateTime;
            var diffReadFromMessagePackFileDateTime = endReadFromMessagePackFileDateTime - beginReadFromMessagePackFileDateTime;
            var diffReadFromSQLiteFileDateTime = endReadFromSQLiteFileDateTime - beginReadFromSQLiteFileDateTime;

            Console.WriteLine(@"\n\n");
            Console.WriteLine($@"diff Json Time        : ${diffReadFromJsonFileDateTime.ToString()}");
            Console.WriteLine($@"diff MessagePack Time : ${diffReadFromMessagePackFileDateTime.ToString()}");
            Console.WriteLine($@"diff SQLite Time      : ${diffReadFromSQLiteFileDateTime.ToString()}");
        }
    }
}
