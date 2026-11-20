# LifeLink Learning Notes: C# Haversine, SQL Server Geography, and Location-Based Donor Matching

**Project:** LifeLink\
**Day:** 4 --- Smart Donor Matching Engine\
**Focus:** Geographic distance, database-side spatial queries,
application architecture, alternatives, and system design

------------------------------------------------------------------------

## 1. What problem are we solving?

LifeLink needs to find donors who may be able to respond to a blood
request. A request contains a hospital location, represented by latitude
and longitude. A donor profile may also contain latitude and longitude.

The matching feature needs to:

1.  Apply the application's approved donor filters.
2.  Calculate how far each eligible donor is from the hospital.
3.  Optionally restrict results to a search radius, such as 10 km or 25
    km.
4.  Sort donors by distance, nearest first.
5.  Return a paginated result to the requester.

For example, if the hospital is at one location and eligible donors are
2 km, 5 km, 8 km, and 15 km away, the distance ordering is 2, 5, 8, 15
km.

**Important safety distinction:** Distance is a logistical
prioritization signal. It does not establish medical compatibility,
donor eligibility, availability at the time of donation, or a safe
transfusion. Blood-group filtering in this learning project is only an
initial software filter. Qualified medical professionals and approved
clinical rules must determine actual compatibility and eligibility.

------------------------------------------------------------------------

## 2. Latitude and longitude from the beginning

The Earth can be represented approximately as a sphere for a basic
distance calculation.

-   **Latitude** describes how far north or south a point is from the
    Equator. Its valid range is -90° to +90°.
-   **Longitude** describes how far east or west a point is from the
    Prime Meridian. Its valid range is -180° to +180°.

A location is represented by a pair:

``` text
(latitude, longitude)
```

For example:

``` text
Hospital: (13.3409, 74.7421)
Donor:    (13.3450, 74.7500)
```

The order matters. Many spatial APIs accept coordinates as
latitude/longitude, while some geometry constructors use X/Y where X is
longitude and Y is latitude. Always check the API's coordinate-order
convention.

### Coordinate quality matters

A valid numeric range does not guarantee a correct location. A user can
accidentally swap latitude and longitude and still produce values that
look numerically valid. The application should:

-   Validate coordinate ranges.
-   Make coordinate order explicit in DTOs and method names.
-   Obtain coordinates from a trusted geocoding or map interaction
    workflow where possible.
-   Avoid silently treating missing coordinates as `(0, 0)`.
-   Record the coordinate source and accuracy when the product needs
    that information.
-   Consider privacy because a donor's home coordinates are sensitive
    location data.

------------------------------------------------------------------------

## 3. Option A: Haversine calculation in C

### 3.1 What is Haversine?

The Haversine formula calculates the great-circle distance between two
points on a sphere from their latitude and longitude.

A great-circle path is the shortest path along the surface of a sphere.
The result is a straight-line surface distance, not the distance a
vehicle will travel along roads.

Let:

-   `φ1` = first latitude in radians
-   `φ2` = second latitude in radians
-   `λ1` = first longitude in radians
-   `λ2` = second longitude in radians
-   `Δφ = φ2 - φ1`
-   `Δλ = λ2 - λ1`
-   `R` = Earth's mean radius, approximately 6,371 km

The formula is:

``` text
a = sin²(Δφ / 2)
    + cos(φ1) × cos(φ2) × sin²(Δλ / 2)

c = 2 × atan2(√a, √(1 - a))

distanceKm = R × c
```

### 3.2 Why convert degrees to radians?

GPS coordinates are normally expressed in degrees. The trigonometric
functions in .NET's `System.Math`, including `Sin`, `Cos`, and `Atan2`,
work with radians.

``` text
radians = degrees × π / 180
```

In C#:

``` csharp
private static double ToRadians(double degrees)
{
    return degrees * (Math.PI / 180.0);
}
```

For example, 180 degrees equals π radians.

### 3.3 Worked example

Use:

``` text
Hospital: latitude 13.3409, longitude 74.7421
Donor:    latitude 13.3450, longitude 74.7500
```

Convert all four values to radians, calculate the differences, calculate
`a`, then `c`, and finally multiply by 6,371 km.

The approximate result is **0.969 km** (about 969 metres). Small
differences in the last decimal place can occur because of rounding and
the chosen Earth-radius constant.

### 3.4 C# implementation

The following is a reusable, framework-independent calculation. It
belongs in an application utility/domain-independent calculation
component, not inside an EF Core repository.

``` csharp
namespace LifeLink.Application.Utilities;

public static class DistanceCalculator
{
    private const double EarthRadiusKm = 6371.0;

    private static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180.0);
    }

    public static double CalculateDistanceKm(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        if (!double.IsFinite(latitude1) ||
            !double.IsFinite(latitude2) ||
            !double.IsFinite(longitude1) ||
            !double.IsFinite(longitude2))
        {
            throw new ArgumentException(
                "Coordinates must contain finite numeric values.");
        }

        if (latitude1 < -90 || latitude1 > 90 ||
            latitude2 < -90 || latitude2 > 90)
        {
            throw new ArgumentOutOfRangeException(
                nameof(latitude1),
                "Latitude must be between -90 and 90 degrees.");
        }

        if (longitude1 < -180 || longitude1 > 180 ||
            longitude2 < -180 || longitude2 > 180)
        {
            throw new ArgumentOutOfRangeException(
                nameof(longitude1),
                "Longitude must be between -180 and 180 degrees.");
        }

        double lat1 = ToRadians(latitude1);
        double lon1 = ToRadians(longitude1);
        double lat2 = ToRadians(latitude2);
        double lon2 = ToRadians(longitude2);

        double deltaLatitude = lat2 - lat1;
        double deltaLongitude = lon2 - lon1;

        double a =
            Math.Pow(Math.Sin(deltaLatitude / 2), 2) +
            Math.Cos(lat1) *
            Math.Cos(lat2) *
            Math.Pow(Math.Sin(deltaLongitude / 2), 2);

        // Protect against tiny floating-point rounding excursions.
        a = Math.Clamp(a, 0.0, 1.0);

        double c = 2 * Math.Atan2(
            Math.Sqrt(a),
            Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }
}
```

### 3.5 Why is this useful?

Advantages:

-   Simple to understand and implement.
-   No database-specific spatial feature is required.
-   Can be unit-tested without SQL Server.
-   Can be used in .NET, Java, Python, JavaScript, Go, and other
    languages.
-   Useful when the application already has a relatively small candidate
    list.

Limitations:

-   If the application loads thousands or millions of donor rows and
    calculates every distance in application memory, it may transfer and
    process far more data than necessary.
-   Sorting and radius filtering happen after data retrieval unless
    additional filtering is performed.
-   It calculates straight-line distance, not road travel distance.
-   A spherical Earth approximation is not as precise as an ellipsoidal
    geodesic calculation over all distances.

**Key lesson:** Haversine is a mathematical algorithm, not a .NET
feature. C# is just one language in which we can implement it.

------------------------------------------------------------------------

## 4. Option B: SQL Server `geography`

### 4.1 What is `geography`?

SQL Server provides spatial data types. The two important types are:

-   `geography`: represents positions and shapes on a round-Earth model.
-   `geometry`: represents planar, flat, Euclidean coordinates.

For latitude/longitude locations on Earth, `geography` is usually the
relevant type.

A `geography` point can represent the hospital or donor location. SQL
Server can then calculate spatial distance using methods such as
`STDistance()`.

### 4.2 What does `STDistance()` do?

`STDistance()` returns the shortest distance between two geography
instances, expressed in the spatial reference system's units. For the
common SRID 4326 geographic reference system, SQL Server returns the
distance in metres.

Conceptual SQL:

``` sql
DECLARE @Hospital geography =
    geography::Point(13.3409, 74.7421, 4326);

DECLARE @Donor geography =
    geography::Point(13.3450, 74.7500, 4326);

SELECT @Hospital.STDistance(@Donor) AS DistanceMeters;
```

**Important coordinate order:** SQL Server's `geography::Point()` takes
latitude first, then longitude, then SRID:

``` sql
geography::Point(latitude, longitude, 4326)
```

This is easy to confuse with APIs that use longitude as X and latitude
as Y.

`4326` identifies the commonly used WGS 84 spatial reference system.

### 4.3 Filter donors within a radius

Conceptual example:

``` sql
DECLARE @Hospital geography =
    geography::Point(13.3409, 74.7421, 4326);

DECLARE @RadiusMeters float = 10000; -- 10 km

SELECT
    dp.Id AS DonorProfileId,
    dp.BloodGroup,
    dp.CityId,
    dp.Latitude,
    dp.Longitude,
    donorLocation.STDistance(@Hospital) AS DistanceMeters
FROM dbo.DonorProfiles AS dp
CROSS APPLY
(
    SELECT geography::Point(
        CAST(dp.Latitude AS float),
        CAST(dp.Longitude AS float),
        4326
    ) AS DonorLocation
) AS locationData
CROSS APPLY
(
    SELECT locationData.DonorLocation AS DonorLocation
) AS locationAlias
WHERE
    dp.IsAvailable = 1
    AND dp.IsVerified = 1
    AND dp.BloodGroup = 'O+'
    AND dp.Latitude IS NOT NULL
    AND dp.Longitude IS NOT NULL
    AND locationAlias.DonorLocation.STDistance(@Hospital)
        <= @RadiusMeters
ORDER BY
    locationAlias.DonorLocation.STDistance(@Hospital),
    dp.Id;
```

This is an illustrative query, not yet the final LifeLink repository
query. It constructs geography values from the existing decimal
coordinate columns. The final query should be simplified and verified
against the actual SQL Server schema and query plan.

### 4.4 Persisted geography column or calculate it from decimal coordinates?

There are two common design choices.

**Choice 1: Keep latitude and longitude columns and construct geography
in a query**

Advantages: - Minimal schema changes. - Reuses the existing LifeLink
schema. - Good for learning and an initial implementation.

Limitations: - Reconstructing the spatial point for each row can add
work. - A spatial index cannot directly accelerate a query against a
point expression in the same way it can against a persisted indexed
geography column.

**Choice 2: Add a persisted or maintained `geography` column**

For example:

``` sql
ALTER TABLE dbo.DonorProfiles
ADD Location geography NULL;
```

The application or database logic must keep `Location` synchronized with
`Latitude` and `Longitude`. A spatial index can then be considered:

``` sql
CREATE SPATIAL INDEX IX_DonorProfiles_Location
ON dbo.DonorProfiles(Location);
```

The exact index configuration, supported options, and query shape should
be validated for the SQL Server version and actual workload. A spatial
index does not automatically make every `STDistance()` query fast; query
predicates and execution plans matter.

For LifeLink, do not add this column immediately. First compare the
simpler query with the existing schema, then decide whether the spatial
column and index are justified.

------------------------------------------------------------------------

## 5. C# Haversine vs SQL Server Geography

  -----------------------------------------------------------------------
  Concern                 C# Haversine            SQL Server `geography`
  ----------------------- ----------------------- -----------------------
  What it is              Mathematical algorithm  Database spatial data
                          implemented in          type and spatial
                          application code        methods

  Calculation location    Application process     Database engine

  Database dependency     None                    SQL Server spatial
                                                  support

  Unit testing            Very straightforward    Usually needs SQL
                                                  Server integration
                                                  testing

  Large candidate sets    Can be inefficient if   Can filter/order close
                          all rows are loaded     to the data
                          first                   

  Spatial indexing        Not provided by the C#  Spatial indexes are
                          method itself           available

  Road distance           No                      No; `STDistance()` is
                                                  not road routing

  Portability             High                    Database-specific

  Best fit                Small lists, reusable   Database-side
                          calculations,           geographic filtering
                          non-database use        and ranking
  -----------------------------------------------------------------------

### Recommended LifeLink approach

Use each where it adds value:

1.  Keep the C# Haversine calculator as a reusable calculation and
    learning component.
2.  Use SQL Server `geography` for database-side distance filtering and
    sorting if the donor dataset and query workload justify it.
3.  Keep the repository responsible for data access and
    database-specific spatial queries.
4.  Keep the application service responsible for orchestrating matching
    rules and pagination.
5.  Do not calculate every donor's distance in C# after loading the
    entire donor table.

We should not calculate the same distance in both C# and SQL for every
production request without a reason. That would add complexity and
duplicate work. The C# implementation can remain independently useful
and testable, while the production repository can use SQL Server for
set-based matching.

------------------------------------------------------------------------

## 6. Is this possible only in .NET?

**No. Absolutely not.**

Location-based distance calculation is not tied to .NET.

Haversine is mathematics and can be implemented in practically any
general-purpose programming language.

Examples:

  -----------------------------------------------------------------------
  Ecosystem                           Typical options
  ----------------------------------- -----------------------------------
  .NET / C#                           Haversine utility, SQL Server
                                      `geography`, NetTopologySuite

  Java                                Haversine libraries, JTS, Hibernate
                                      Spatial

  Python                              `math`, geopy, Shapely, GeoDjango,
                                      PostGIS

  JavaScript / TypeScript             Turf.js, geospatial packages,
                                      browser map SDKs

  PostgreSQL                          PostGIS, `ST_Distance`,
                                      `ST_DWithin`

  MySQL                               Spatial data types and spatial
                                      functions

  MongoDB                             GeoJSON and geospatial indexes such
                                      as `2dsphere`

  Cloud mapping services              Google Maps Platform, Mapbox, HERE,
                                      Azure Maps
  -----------------------------------------------------------------------

The exact features and semantics differ. For example, a database's
spatial distance function may use a spherical or spheroidal model and
different units depending on the spatial type and reference system.

------------------------------------------------------------------------

## 7. Is this technology used by large applications?

Yes. Geospatial search is a well-established capability used in many
categories of large-scale systems:

-   Ride-hailing: finding nearby drivers and estimating pickup options.
-   Food delivery: identifying restaurants and couriers near a customer.
-   Logistics: locating warehouses, vehicles, and delivery destinations.
-   Travel and maps: searching places and calculating routes.
-   Retail: finding nearby stores and service locations.
-   Emergency response: locating resources near an incident.
-   Healthcare: finding nearby hospitals, clinics, pharmacies, or
    potential donors.

Well-known products such as Uber, DoorDash, Google Maps, and Airbnb have
location-centric product features. Their exact internal architectures
and technology choices are not established by this example; large
companies commonly use multiple databases, caches, geospatial indexes,
routing systems, and custom services. Do not assume they use SQL Server
`geography` or the same Haversine implementation as LifeLink.

### Important distinction: distance versus routing

A proximity calculation answers:

> How far apart are these two points geographically?

A routing engine answers:

> What route can a person take, and how long might that journey take?

For example:

-   Haversine: 5 km straight-line distance.
-   Road route: perhaps 8 km and 18 minutes.

Road distance and travel time require routing data and often a routing
engine or external provider.

------------------------------------------------------------------------

## 8. Is this a tech stack?

No. Haversine and SQL Server `geography` are not a complete technology
stack.

They are different pieces of a geospatial solution:

  -----------------------------------------------------------------------
  Item                                Category
  ----------------------------------- -----------------------------------
  Haversine                           Mathematical algorithm

  C#                                  Programming language

  .NET                                Application framework/runtime
                                      ecosystem

  SQL Server                          Relational database

  `geography`                         SQL Server spatial data type

  `STDistance()`                      Spatial distance method

  Spatial index                       Database indexing capability

  PostGIS                             PostgreSQL spatial extension

  Turf.js                             JavaScript geospatial library

  Google Maps / Mapbox / HERE         Mapping, geocoding, routing and
                                      related services
  -----------------------------------------------------------------------

A possible LifeLink stack is:

``` text
React + JavaScript
       |
       | HTTPS API
       v
ASP.NET Core Web API (.NET)
       |
       v
Application Layer
(Matching use case, radius, pagination)
       |
       v
Infrastructure / Repository
(EF Core + SQL spatial query)
       |
       v
SQL Server
(Users, DonorProfiles, BloodRequests,
 geography operations and indexes)
```

This is a stack and architecture that uses geospatial capabilities.
Haversine and `geography` are components within it.

------------------------------------------------------------------------

## 9. How should we design LifeLink's matching system?

### 9.1 High-level architecture

``` text
React frontend
     |
     | POST/GET matching request
     v
ASP.NET Core API
     |
     v
DonorMatchingService
     |
     |-- Validate request and requester access
     |-- Apply matching policy
     |-- Set radius and pagination
     |
     v
IDonorMatchingRepository
     |
     v
DonorMatchingRepository
     |
     |-- Filter available donors
     |-- Filter verified donors
     |-- Apply approved blood-group rule
     |-- Exclude missing coordinates
     |-- Calculate distance in SQL
     |-- Apply radius
     |-- Sort nearest first
     |-- Paginate
     |
     v
SQL Server
     |
     v
Matching results
     |
     v
API response with donor details and distance
```

### 9.2 Clean Architecture responsibilities

**Domain** - Existing donor and blood request concepts. - Domain rules
that genuinely belong to the business domain. - Do not put EF Core or
SQL Server spatial types here.

**Application** - `IDonorMatchingService`. - Matching query/criteria
DTOs. - Matching result DTOs. - Orchestration and validation of use-case
inputs. - Pagination policy and application-level rules.

**Infrastructure** - `IDonorMatchingRepository` implementation. - EF
Core and SQL Server-specific spatial query. - Mapping database results
into application result models.

**API** - Authenticated endpoint. - Request/query binding. - Requester
authorization. - HTTP response.

The Application layer should not depend directly on
`Microsoft.EntityFrameworkCore` or SQL Server spatial types.

### 9.3 Example response model

A matching endpoint could return:

``` json
{
  "bloodRequestId": 42,
  "searchRadiusKm": 10,
  "totalCount": 2,
  "pageNumber": 1,
  "pageSize": 20,
  "donors": [
    {
      "donorProfileId": 12,
      "displayName": "Donor A",
      "bloodGroup": "O+",
      "city": "Udupi",
      "distanceKm": 0.969
    },
    {
      "donorProfileId": 18,
      "displayName": "Donor B",
      "bloodGroup": "O+",
      "city": "Mangaluru",
      "distanceKm": 4.2
    }
  ]
}
```

This is an illustrative contract. Do not expose private donor home
coordinates, personal phone numbers, or exact addresses in a public
matching response. Return only the information needed for the authorized
workflow.

### 9.4 Suggested repository query behavior

The repository should perform the work as close to the data as
practical:

1.  Find eligible donors using database filters.
2.  Exclude missing or invalid coordinates.
3.  Calculate distance to the hospital.
4.  Filter by radius.
5.  Sort by distance ascending.
6.  Use a deterministic tie-breaker such as donor profile ID.
7.  Count matching records and return the requested page.

A stable ordering matters for pagination. Sorting only by distance can
produce unstable page boundaries when two donors have equal distances.
Add a deterministic secondary sort.

### 9.5 Performance considerations

For a small project database, a straightforward spatial query may be
sufficient. For a large production dataset, consider:

-   Spatial indexes and query-plan analysis.
-   A bounding-box prefilter before precise distance calculation.
-   Filtering by approved donor attributes before expensive
    calculations.
-   Pagination in the database.
-   Avoiding retrieval of unused donor columns.
-   Caching only when freshness and privacy requirements permit it.
-   Rate limiting and authorization to prevent location-data scraping.
-   Measuring actual query performance instead of assuming an index
    helps.

A bounding-box prefilter is a coarse geographical rectangle around the
hospital. It reduces the candidate set; the exact distance calculation
then filters the remaining candidates. It must handle latitude
boundaries and longitude wraparound correctly.

### 9.6 Distance is not travel time

The matching result can include straight-line distance, but it must not
be presented as an arrival-time guarantee.

If LifeLink later needs estimated travel time, the architecture can call
a routing provider:

``` text
Matching service
      |
      +--> Spatial database: nearby eligible donors
      |
      +--> Routing provider: road distance / ETA
```

Use a routing provider only for a suitably small candidate list because
route-matrix calls may have cost, rate limits, latency, and privacy
implications.

------------------------------------------------------------------------

## 10. Alternatives and when to choose them

### A. PostGIS (PostgreSQL)

PostGIS is a mature spatial extension for PostgreSQL. It provides
spatial data types, functions, and indexes.

Consider it when: - PostgreSQL is your preferred database. - You need
advanced geospatial querying. - You need a mature open-source spatial
ecosystem.

Common functions include: - `ST_Distance` - `ST_DWithin` -
`ST_GeographyFromText` - Spatial indexes using GiST.

### B. MongoDB geospatial indexes

MongoDB supports GeoJSON points and `2dsphere` indexes.

Consider it when: - Your application already uses MongoDB. - Your data
model fits a document database. - You need geospatial queries alongside
document-oriented access.

### C. MySQL spatial capabilities

MySQL supports spatial types and functions. It can be a fit when MySQL
is already part of the system and the spatial requirements match its
capabilities.

### D. NetTopologySuite

NetTopologySuite is a .NET library for spatial data types and
operations. It is useful when spatial calculations and geometry
operations are needed in .NET.

It is not itself a database. EF Core providers can integrate with
spatial types, but provider support and translation capabilities must be
checked.

### E. Turf.js

Turf.js is a JavaScript geospatial analysis library. It can calculate
distances and perform other spatial operations in frontend or Node.js
code.

For LifeLink, do not trust a browser-side calculation as the
authoritative matching result. The backend/database must enforce the
actual matching rules.

### F. External mapping and routing services

Examples include Google Maps Platform, Mapbox, HERE, and Azure Maps.

These services can provide combinations of: - Geocoding: address to
coordinates. - Reverse geocoding: coordinates to an address. - Routing:
route between locations. - Distance matrices: road distance and
estimated travel time. - Map tiles and visual maps.

They are not replacements for donor eligibility or database matching
logic. They also introduce external cost, quotas, latency, and
data-sharing considerations.

------------------------------------------------------------------------

## 11. Which approach should LifeLink use?

For the current project:

**Recommended initial design** - Keep SQL Server as the database. - Keep
latitude and longitude in the existing schema. - Keep the C# Haversine
implementation as an independently testable calculation. - Evaluate SQL
Server `geography` and `STDistance()` for filtering and ranking in the
repository. - Do not add a spatial column or index until the query
design and performance need are understood. - Keep road routing and ETA
outside the first version.

Why this is a sensible learning design: - It preserves the existing .NET
and SQL Server architecture. - It teaches both application-side
mathematics and database-side spatial querying. - It avoids premature
database schema changes. - It creates a clear path to spatial indexing
if the donor dataset grows.

For a real production decision, benchmark representative data volumes
and inspect SQL Server execution plans before choosing the final
implementation.

------------------------------------------------------------------------

## 12. Security, privacy, and medical considerations

Location data can be sensitive. LifeLink should:

-   Require authentication and authorization before returning matching
    donors.
-   Return only the minimum donor information required.
-   Avoid exposing exact donor coordinates or home addresses to
    requesters.
-   Consider showing approximate distance rather than precise location.
-   Obtain appropriate consent for collecting and using donor location.
-   Define retention and deletion rules for location data.
-   Avoid logging sensitive coordinates unnecessarily.
-   Protect APIs against scraping and excessive search requests.
-   Keep medical eligibility and compatibility decisions under approved
    clinical workflows.

A donor marked available in the application is not necessarily medically
eligible or available at the moment of donation. The application should
not promise that a donor will donate or that a particular blood group is
clinically compatible.

------------------------------------------------------------------------

## 13. Interview-ready questions

### Q1. What is the Haversine formula?

It calculates the great-circle distance between two geographical points
using latitude and longitude, assuming a spherical Earth.

### Q2. Why do we convert degrees to radians?

The trigonometric functions in .NET's `System.Math` expect angles in
radians.

### Q3. What is SQL Server `geography`?

It is a SQL Server spatial data type for representing geographical
coordinates and shapes on a round-Earth model.

### Q4. What does `STDistance()` return?

It returns the shortest distance between two geography instances in the
spatial reference system's units. For SRID 4326, SQL Server reports the
distance in metres.

### Q5. What is the difference between `geometry` and `geography`?

`geometry` models planar, flat coordinates. `geography` models
round-Earth geographical coordinates.

### Q6. Why calculate distance in the database?

It can filter, sort, and paginate close to the stored data, avoiding the
need to transfer every candidate row into application memory.

### Q7. Does Haversine return road distance?

No. It returns great-circle surface distance. Road distance and ETA
require routing data or a routing service.

### Q8. Is Haversine specific to .NET?

No. It is a mathematical algorithm that can be implemented in any
language.

### Q9. What is a spatial index?

A database index designed to accelerate spatial queries. It helps the
database identify candidate geographical objects without scanning every
row, subject to query shape and index support.

### Q10. What would you choose for a large donor-matching system?

I would evaluate the database's spatial capabilities, use spatial
filtering and indexing where appropriate, paginate in the database, and
benchmark with representative data. I would use a routing engine
separately if road travel time is required.

------------------------------------------------------------------------

## 14. Day 4 implementation checklist

-   [x] Define location-based matching across neighbouring cities.
-   [x] Understand latitude and longitude.
-   [x] Understand degrees and radians.
-   [x] Learn the Haversine formula.
-   [x] Work through a distance calculation.
-   [x] Implement the reusable C# `DistanceCalculator`.
-   [ ] Understand SQL Server `geography` and `STDistance()`.
-   [ ] Decide the final database query strategy.
-   [ ] Create donor-matching query/result models.
-   [ ] Implement repository filtering, distance, radius, sorting, and
    pagination.
-   [ ] Implement the application service.
-   [ ] Add the authenticated matching API.
-   [ ] Provide the complete unit and integration test files together.
-   [ ] Review performance, privacy, and authorization.
-   [ ] Complete the final build and Day 4 review.

------------------------------------------------------------------------

## 15. Final mental model

Remember these distinctions:

``` text
Haversine
    = mathematical algorithm

C#
    = language used to implement the algorithm

.NET
    = application framework/runtime

SQL Server geography
    = database spatial data type

STDistance()
    = database spatial distance method

Spatial index
    = database optimization capability

PostGIS / MongoDB geospatial
    = alternative database spatial technologies

Google Maps / Mapbox / HERE
    = mapping, geocoding, and/or routing platforms
```

**The central architecture principle:** The Application layer defines
the donor-matching use case; Infrastructure performs database-specific
spatial querying; SQL Server filters and ranks candidate donors; the API
returns only authorized, privacy-conscious results.
