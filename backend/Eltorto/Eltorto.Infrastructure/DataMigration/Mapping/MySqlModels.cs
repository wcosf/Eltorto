namespace Eltorto.Infrastructure.DataMigration.Mappings;

// Модели для данных из MySQL (старая структура)
public class MySqlCategory
{
    public int id { get; set; }
    public string cat { get; set; } = string.Empty;
    public string catrus { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
    public string sorto { get; set; } = "0";
}

public class MySqlCake
{
    public int id { get; set; }
    public string smfoto { get; set; } = string.Empty;
    public string foto { get; set; } = string.Empty;
    public string category { get; set; } = string.Empty;
    public string? podcategory { get; set; }
    public string? luch { get; set; }
    public string nazvanie { get; set; } = string.Empty;
    public string? commo { get; set; }
}

public class MySqlFilling
{
    public int id { get; set; }
    public string? nazv { get; set; }
    public string? nach { get; set; }
    public string kart { get; set; } = string.Empty;
    public string? razrez { get; set; }
}

public class MySqlTestimonial
{
    public int id { get; set; }
    public string data { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
    public string? text { get; set; }
    public string? otvet { get; set; }
    public string razresh { get; set; } = "0";
}

public class MySqlPage
{
    public int id { get; set; }
    public string name { get; set; } = string.Empty;
    public string name_rus { get; set; } = string.Empty;
    public string title { get; set; } = string.Empty;
    public string keywords { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public string zagol { get; set; } = string.Empty;
    public string podzagol { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
    public string klutch { get; set; } = "0";
    public string tip { get; set; } = string.Empty;
    public string sorto { get; set; } = "0";
}

public class MySqlSlider
{
    public int id { get; set; }
    public string kart { get; set; } = string.Empty;
    public string zagol { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
    public string sorti { get; set; } = "0";
}

public class MySqlContact
{
    public int id { get; set; }
    public string telone { get; set; } = string.Empty;
    public string teltwo { get; set; } = string.Empty;
    public string adres { get; set; } = string.Empty;
    public string email { get; set; } = string.Empty;
    public string karta { get; set; } = string.Empty;
}

public class MySqlInfoBlock
{
    public int id { get; set; }
    public string kart { get; set; } = string.Empty;
    public string zagol { get; set; } = string.Empty;
    public string content { get; set; } = string.Empty;
    public string klutch { get; set; } = "0";
    public string sorto { get; set; } = "0";
}
