using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ImportDto
{
    [XmlType("Officer")]
    public class OfficerDto
    {
        [StringLength(30, MinimumLength = 3)]
        [Required]
        [XmlElement("Name")]
        public string Name { get; set; }


        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        [XmlElement("Money")]
        public decimal Money { get; set; }

        [Required]
        [XmlElement("Position")]
        public string Position { get; set; }

        [Required]
        [XmlElement("Weapon")]
        public string Weapon { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [XmlArray("Prisoners")]
        public PrisonerXmlDto[] Prisoners { get; set; }
    }

    [XmlType("Prisoner")]
    public class PrisonerXmlDto
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
    }
}
