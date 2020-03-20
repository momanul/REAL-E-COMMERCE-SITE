# REAL-E-COMMERCE-SITE

add-migration u1 -Context DataDbContext
add-migration u2 -Context MyAppIdentityDbContext

update-database -Context DataDbContext
update-database -Context MyAppIdentityDbContext
