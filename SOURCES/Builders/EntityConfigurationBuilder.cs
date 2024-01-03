using SOURCES.Builders.Abstract;
using SOURCES.Models;
using SOURCES.Workers;

namespace SOURCES.Builders;

// ReSharper disable once UnusedType.Global
public class EntityConfigurationBuilder : ISourceBuilder
{
    public void BuildSourceFile(List<Entity> entities)
    {
        entities.ForEach(model =>
        {
            if (model.Configure)
                SourceBuilder.Instance.AddSourceFile(Constants.EntityConfigurationPath, $"{model.Name}Configuration.cs",
                    BuildSourceText(model, null));
        });
    }

    public string BuildSourceText(Entity? entity, List<Entity>? entities)
    {
        var text = """
                   using ENTITIES.Entities{entityPath};
                   using Microsoft.EntityFrameworkCore;
                   using Microsoft.EntityFrameworkCore.Metadata.Builders;

                   namespace DAL.EntityFramework.Configurations;

                   public class {entityName}Configuration : IEntityTypeConfiguration<{entityName}>
                   {
                       public void Configure(EntityTypeBuilder<{entityName}> builder)
                       {
                           
                       }
                   }

                   """;
        text = text.Replace("{entityName}", entity!.Name);
        text = text.Replace("{entityPath}", !string.IsNullOrEmpty(entity!.Path) ? $".{entity.Path}" : string.Empty);
        return text;
    }
}