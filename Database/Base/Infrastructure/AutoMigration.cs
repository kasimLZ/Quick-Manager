using Database.Base.Interface.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Database.Base.Infrastructure
{
    public class AutoMigration
    {
        private readonly IServiceProvider _serviceProvider;
        private ApplicationDB _context;

        public AutoMigration(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _context = serviceProvider.GetService<ApplicationDB>();
        }

        public void Migrator()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\Migrations\\");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                Directory.GetFiles(path).ToList().ForEach(File.Delete);
            }

            using (_context)
            {
                var services = ((IInfrastructure<IServiceProvider>)_context).Instance;
                var code = new CSharpHelper();
                var scaffolder = ActivatorUtilities.CreateInstance<MigrationsScaffolder>(services,
                     new CSharpMigrationsGenerator(
                            new MigrationsCodeGeneratorDependencies(),
                            new CSharpMigrationsGeneratorDependencies(
                                code,
                                new CSharpMigrationOperationGenerator(
                                    new CSharpMigrationOperationGeneratorDependencies(code)),
                                new CSharpSnapshotGenerator(new CSharpSnapshotGeneratorDependencies(code))))
                   );
                var projectDir = Path.Combine(path, "..\\");
                var migrationAssembly = new MigrationsAssembly(new CurrentDbContext(_context), new DbContextOptions<ApplicationDB>(), new MigrationsIdGenerator());
                scaffolder.GetType().GetField("_migrationsAssembly", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scaffolder, migrationAssembly);

                var readonlyDic = new ReadOnlyDictionary<string, TypeInfo>(new Dictionary<string, TypeInfo>());
                migrationAssembly.GetType().GetField("_migrations", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(migrationAssembly, new LazyRef<IReadOnlyDictionary<string, TypeInfo>>(readonlyDic));
                var migration = scaffolder.ScaffoldMigration("Information.Migrations", "Information");
                scaffolder.Save(projectDir, migration, path);
            }
           

            using (_context = (ApplicationDB)_serviceProvider.GetService<IApplicationDB>())
            {
                _context.Database.Migrate();
            }
        }

        //private MigrationsScaffolder CreateMigrationScaffolder<TContext>()
        //    where TContext : DbContext, new()
        //{
        //    var currentContext = new CurrentDbContext(new TContext());
        //    var idGenerator = new MigrationsIdGenerator();
        //    var code = new CSharpHelper();
        //    var reporter = new AutoOperationReporter();

        //    var services = ((IInfrastructure<IServiceProvider>)_context).Instance;

        //    return new MigrationsScaffolder(
        //        new MigrationsScaffolderDependencies(
        //            currentContext,
        //            new Model(),
        //            new MigrationsAssembly(
        //                currentContext,
        //                new DbContextOptions<TContext>(),
        //                idGenerator),
        //            new MigrationsModelDiffer(
        //                new TestRelationalTypeMapper(new RelationalTypeMapperDependencies()),
        //                new MigrationsAnnotationProvider(new MigrationsAnnotationProviderDependencies())),
        //            idGenerator,
        //            new CSharpMigrationsGenerator(
        //                new MigrationsCodeGeneratorDependencies(),
        //                new CSharpMigrationsGeneratorDependencies(
        //                    code,
        //                    new CSharpMigrationOperationGenerator(
        //                        new CSharpMigrationOperationGeneratorDependencies(code)),
        //                    new CSharpSnapshotGenerator(new CSharpSnapshotGeneratorDependencies(code)))),
        //            new MockHistoryRepository(),
        //            reporter,
        //            new MockProvider(),
        //            new SnapshotModelProcessor(reporter)));
        //}
    }

    public class AutoOperationReporter : IOperationReporter
    {
        private readonly List<string> _messages = new List<string>();

        public IReadOnlyList<string> Messages => _messages;

        public void Clear() => _messages.Clear();

        public void WriteInformation(string message)
            => _messages.Add("info: " + message);

        public void WriteVerbose(string message)
            => _messages.Add("verbose: " + message);

        public void WriteWarning(string message)
            => _messages.Add("warn: " + message);

        public void WriteError(string message)
            => _messages.Add("error: " + message);
    }
}
