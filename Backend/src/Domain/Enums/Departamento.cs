using System;
using System.Collections.Generic;
using System.Linq;
using IERIC.SumariosIERIC.Domain.SeedWork;

namespace IERIC.SumariosIERIC.Domain.Enums;
public class Departamento : Enumeration
{
    public Departamento(int id, string nombre) : base(id, nombre)
    {
    }
    public static Departamento Ieric = new Departamento(1, nameof(Ieric).ToLowerInvariant());
    public static Departamento Central = new Departamento(2, nameof(Central).ToLowerInvariant());
    public static Departamento Delegacion = new Departamento(3, nameof(Delegacion).ToLowerInvariant());

    public static IEnumerable<Departamento> List() =>
               new[] { Ieric, Central, Delegacion };
    public static Departamento FromName(string nombre)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Nombre, nombre, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new Exception($"Possible values for Departamento: {String.Join(",", List().Select(s => s.Nombre))}");
        }

        return state;
    }

    public static Departamento From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new Exception($"Possible values for Departamento: {String.Join(",", List().Select(s => s.Nombre))}");
        }

        return state;

    }

}