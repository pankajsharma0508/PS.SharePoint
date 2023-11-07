// See https://aka.ms/new-console-template for more information
using PS.SharePoint.Core.Entities;
using PS.SharePoint.Core.Interfaces;
using PS.SharePoint.Core.Test;
using Unity;
using PS.SharePoint.Core.Extensions;


var container = new UnityContainer();
container.RegisterSpConfiguration(new SharePointConfiguration("http://icp054dd.dev.sg.info/Statehome/"));
container.RegisterSpRepository<PackageUserRole>();

var userRoleRepository = container.Resolve<ISharePointRepository<PackageUserRole>>();

userRoleRepository.Create(new PackageUserRole
{
    Title = "Test",
    Age = 1,
    BirthDate = DateTime.Now,
    Description = "Test",
    IsEnrolled = true,
});

userRoleRepository.Create(new PackageUserRole
{
    Title = "Test",
    Age = 1,
    BirthDate = DateTime.Now,
    Description = "Test",
    IsEnrolled = true,
});

var users = userRoleRepository.Get(string.Empty);

var userToDelete = users.First();
userRoleRepository.Delete(userToDelete, true);

foreach (var user in users)
{
    Console.WriteLine(Convert.ToString(user));
}

Console.ReadKey();