using GRAPHQL.Roles;
using GraphQL.Server.Ui.Voyager;

namespace API.Extensions;

/// <summary>
///     Registers the HotChocolate GraphQL server with the Role query/mutation types, projections,
///     sorting, and filtering. <c>UseGraphQLEndpoints</c> maps the schema at <c>/graphql</c>
///     (requires authentication) and mounts GraphQL Voyager at <c>/graphql-voyager</c>.
/// </summary>
public static class GraphQlExtensions
{
    public static IServiceCollection AddGraphQlSchema(this IServiceCollection services)
    {
        services.AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddType<RoleType>()
            .AddType<PermissionType>()
            .AddProjections()
            .AddSorting()
            .AddFiltering();

        return services;
    }

    public static WebApplication UseGraphQlEndpoints(this WebApplication app)
    {
        app.MapGraphQL((PathString)"/graphql").RequireAuthorization();
        app.UseGraphQLVoyager("/graphql-voyager", new VoyagerOptions { GraphQLEndPoint = "/graphql" });
        return app;
    }
}