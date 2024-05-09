using InventorySystem.Application.Extension.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventorySystem.Infrastructure.DataAccess;
public class AppDbContext(DbContextOptions<AppDbContext> options) : 
    IdentityDbContext<ApplicationUser>(options)
{

}
