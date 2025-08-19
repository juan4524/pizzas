public static class DbInicializador
{
    public static void Sembrar(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PizzeriaContext>();

        if (!db.Productos.Any())
        {
            db.Productos.AddRange(
                new Producto { Id="pizza-pepperoni", Nombre="Pizza Pepperoni", Tipo="pizza", Precio=140, Activo=true },
                new Producto { Id="pizza-hawaiana",  Nombre="Pizza Hawaiana",  Tipo="pizza", Precio=150, Activo=true },
                new Producto { Id="coca-2l",         Nombre="Coca Cola 2L",   Tipo="bebida", Precio=45, Activo=true },
                new Producto { Id="pepsi-2l",        Nombre="Pepsi 2L",       Tipo="bebida", Precio=42, Activo=true },
                new Producto { Id="palitos-pan",     Nombre="Palitos de pan", Tipo="snack",  Precio=35, Activo=true },
                new Producto { Id="combo-familiar",  Nombre="Combo familiar", Tipo="combo",  Precio=299, Activo=true }
            );
            db.SaveChanges();
        }
    }
}