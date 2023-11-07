using PS.SharePoint.Core.Entities;
using PS.SharePoint.Core.Extensions;
using PS.SharePoint.Core.Interfaces;
using System;
using System.Linq;
using Unity;

namespace PS.SharePoint.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer();
            container.RegisterSpConfiguration(new SharePointConfiguration("http://icp054dd.dev.sg.info/Statehome/"));
            container.RegisterSpRepository<PackageUserRole>();

            var userRoleRepository = ServiceConfiguration.GetContainer().Resolve<ISharePointRepository<PackageUserRole>>();

            userRoleRepository.Create(new PackageUserRole
            {
                Title = "Test",
                Age = 1,
                BirthDate = DateTime.Now,
                Description = "Test",
                IsEnrolled = true,
                State = "Andorra"
            });

            userRoleRepository.Create(new PackageUserRole
            {
                Title = "Test1",
                Age = 1,
                BirthDate = DateTime.Now,
                Description = "Test",
                IsEnrolled = true,
                State = "Andorra"
            });

            var users = userRoleRepository.Get(string.Empty);

            var userToDelete = users.First();
            userRoleRepository.Delete(userToDelete, true);

            foreach (var user in users)
            {
                Console.WriteLine(Convert.ToString(user));
            }

        }
    }
}
