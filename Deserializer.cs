namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.Data.Models;
    using SoftJail.Data.Models.Enums;
    using SoftJail.DataProcessor.ImportDto;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Deserializer
    {
        

        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var allDepartments = JsonConvert.DeserializeObject<Department[]>(jsonString);

            var validDepartments = new List<Department>();

            var sb = new StringBuilder();

            foreach (var department in allDepartments)
            {
                if (IsValid(department) && department.Cells.All(IsValid))
                {
                    validDepartments.Add(department);
                    sb.AppendLine($"Imported {department.Name} with {department.Cells.Count} cells");
                }
                else
                {
                    sb.AppendLine("Invalid Data");
                }
            }

            context.Departments.AddRange(validDepartments);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var allPrisoners = JsonConvert.DeserializeObject<PrisonerDto[]>(jsonString);

            var validPrisoners = new List<Prisoner>();

            var sb = new StringBuilder();

            foreach (var prisoner in allPrisoners)
            {
                if (IsValid(prisoner) && prisoner.Mails.All(IsValid))
                {
                    var validPrisoner = new Prisoner
                    {
                        FullName = prisoner.FullName,
                        Nickname = prisoner.Nickname,
                        Age = prisoner.Age,
                        IncarcerationDate = DateTime.ParseExact(prisoner.IncarcerationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        ReleaseDate = prisoner.ReleaseDate == null ? new DateTime?()  
                                :DateTime.ParseExact(prisoner.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        Bail = prisoner.Bail,
                        CellId = prisoner.CellId,
                        Mails = prisoner.Mails.Select(x => new Mail
                        {
                            Description = x.Description,
                            Address = x.Address,
                            Sender = x.Sender
                        }).ToArray()
                    };

                    validPrisoners.Add(validPrisoner);
                    sb.AppendLine($"Imported {validPrisoner.FullName} {validPrisoner.Age} years old");
                }
                else
                {
                    sb.AppendLine("Invalid Data");
                }
            }

            context.Prisoners.AddRange(validPrisoners);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(OfficerDto[]), new XmlRootAttribute("Officers"));
            var allOfficers = (OfficerDto[])serializer.Deserialize(new StringReader(xmlString));

            var validOfficers = new List<Officer>();

            var sb = new StringBuilder();

            foreach (var dto in allOfficers)
            {
                var isValidWeapon = Enum.TryParse(dto.Weapon, out Weapon weapon);
                var isValidPosition = Enum.TryParse(dto.Position, out Position position);

                var isValid = IsValid(dto) && isValidPosition && isValidWeapon;

                if (isValid)
                {
                    var officer = new Officer
                    {
                        FullName = dto.Name,
                        Salary = dto.Money,
                        Position = position,
                        Weapon = weapon,
                        DepartmentId = dto.DepartmentId,
                        OfficerPrisoners = dto.Prisoners.Select(x => new OfficerPrisoner
                        {
                            PrisonerId = x.Id
                        }).ToArray()
                    };

                    validOfficers.Add(officer);
                    sb.AppendLine($"Imported {officer.FullName} ({officer.OfficerPrisoners.Count} prisoners)");
                }
                else
                {
                    sb.AppendLine("Invalid Data");
                }
            }

            context.Officers.AddRange(validOfficers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new ValidationContext(obj);
            var validationResults = new List<ValidationResult>();

            return Validator.TryValidateObject(obj, validationContext, validationResults, true);
        }

    }
}