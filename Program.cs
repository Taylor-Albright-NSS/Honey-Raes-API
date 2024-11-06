using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;
using Microsoft.AspNetCore.Authentication;
List<Customer> customers = new List<Customer> ()
{
    new Customer()
    {
        Id = 1,
        Name = "Timid Teedle",
        Address = "343 Taxidermist Ln"
    },
    new Customer()
    {
        Id = 2,
        Name = "Steven Scuzzle",
        Address = "999 Mountain Top Pass"
    },
    new Customer()
    {
        Id = 3,
        Name = "Crinkle Crunkle",
        Address = "195 Crooked Ave"
    },
};
List<Employee> employees = new List<Employee>()
{
    new Employee()
    {
        Id = 1,
        Name = "Tod Shmod",
        Specialty = "Speed"
    },
    new Employee()
    {
        Id = 2,
        Name = "Clarence McClarren",
        Specialty = "Precise"
    },
};

List<ServiceTicket> serviceTickets = new List<ServiceTicket>()
{
    new ServiceTicket()
    {
        Id = 1,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Can't log in",
        Emergency = true,
        DateCompleted = new DateTime(2024, 11, 1)
    },
    new ServiceTicket()
    {
        Id = 2,
        CustomerId = 2,
        Description = "Can't submit post",
        Emergency = false,
    },
    new ServiceTicket()
    {
        Id = 3,
        CustomerId = 3,
        Description = "Can't see anything!",
        Emergency = false,
        DateCompleted = new DateTime(2024, 11, 1)
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 3,
        EmployeeId = 2,
        Description = "Your layout is terrible and I want it fixed.",
        Emergency = false,
    },
    new ServiceTicket()
    {
        Id = 5,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "Video won't load",
        Emergency = false,
        DateCompleted = new DateTime(2024, 11, 1)
    },
};

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Put endpoints here
app.MapGet("/customers", () =>
{
    return customers.Select(c => new CustomerDTO
    {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address,
    });
});
app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(customer => customer.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
    });
});


app.MapGet("/employees/", () =>
{
    return employees.Select(e => new EmployeeDTO
    {
        Id = e.Id,
        Name = e.Name,
        Specialty = e.Specialty,
    });
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    List<ServiceTicket> tickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(new EmployeeDTO
    {
        Id = employee.Id,
        Name = employee.Name,
        Specialty = employee.Specialty,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});


app.MapGet("/servicetickets", () =>
{
    return serviceTickets.Select(t => new ServiceTicketDTO
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        EmployeeId = t.EmployeeId,
        Description = t.Description,
        Emergency = t.Emergency,
        DateCompleted = t.DateCompleted,
    });
});
app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (serviceTicket == null)
    {
        return Results.NotFound();
    }

    Employee employee = employees.FirstOrDefault(emp => emp.Id == serviceTicket.EmployeeId);
    Customer customer = customers.FirstOrDefault(cust => cust.Id == serviceTicket.CustomerId);
    return Results.Ok(new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        EmployeeId = serviceTicket.EmployeeId,
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency,
        DateCompleted = serviceTicket.DateCompleted,
        Employee = employee == null ? null : new EmployeeDTO
        {
            Id = employee.Id,
            Name = employee.Name,
            Specialty = employee.Specialty
        },
        Customer = customer == null ? null : new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address,
        }
    });
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) => {
    Customer customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    if (customer == null)
    {
        return Results.BadRequest();
    }

    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);

    return Results.Created($"/servicetickets/{serviceTicket.Id}", new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency
    });

});

app.MapDelete("/servicetickets/{id}", (int id) => 
{
    ServiceTicket ticketToDelete = serviceTickets.FirstOrDefault(ticket => ticket.Id == id);
    
        if (ticketToDelete == null)
    {
        // Return a 404 Not Found response if the ticket does not exist
        return Results.NotFound($"Service ticket with ID {id} not found.");
    }

    serviceTickets.Remove(ticketToDelete);
    return Results.NoContent();
});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }

    ticketToUpdate.CustomerId = serviceTicket.CustomerId;
    ticketToUpdate.EmployeeId = serviceTicket.EmployeeId;
    ticketToUpdate.Description = serviceTicket.Description;
    ticketToUpdate.Emergency = serviceTicket.Emergency;
    ticketToUpdate.DateCompleted = serviceTicket.DateCompleted;

    return Results.NoContent();
});

app.MapPost("/servicetickets/{id}/complete", (int id) => 
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);

    ticketToComplete.DateCompleted = DateTime.Now;
});


app.Run();



