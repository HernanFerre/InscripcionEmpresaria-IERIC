using System;
using System.Collections.Generic;
using System.Linq;


public class Rol
{

    public string Nombre { get; private set; }

    public int Id { get; private set; }

    protected Rol(int id, string nombre)
    {
        Id = id;
        Nombre = nombre;
    }
    public static Rol Sin_Permisos = new Rol(0, nameof(Sin_Permisos).ToLowerInvariant());
    public static Rol Administrador = new Rol(1, nameof(Administrador).ToLowerInvariant());
    public static Rol Supervisor = new Rol(2, nameof(Supervisor).ToLowerInvariant());
    public static Rol Usuario = new Rol(3, nameof(Usuario).ToLowerInvariant());


    public static IEnumerable<Rol> List() => new[] { Sin_Permisos, Administrador, Supervisor, Usuario };


    public static Rol FromName(string nombre)
    {
        var state = List().SingleOrDefault(s => String.Equals(s.Nombre, nombre, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new Exception($"Valores posibles para el Rol: {String.Join(",", List().Select(s => s.Nombre))}");
        }

        return state;
    }

    public static Rol From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new Exception($"Valores posibles para el Rol: {String.Join(",", List().Select(s => s.Nombre))}");
        }

        return state;
    }
    public static string ToStringRepresentation(List<Rol> roles)
    {
        if (roles == null) return "";
        var retorno = "";
        foreach (Rol rol in roles)
        {
            retorno = retorno + ";" + rol.Nombre;
        }
        return retorno.Substring(1);
    }

    public static List<Rol> FromStringRepresentation(string roles)
    {
        var retorno = new List<Rol>();
        foreach (string rol in roles.Split(";"))
        {
            if (rol != "") retorno.Add(Rol.FromName(rol));
        }
        return retorno;
    }
}


