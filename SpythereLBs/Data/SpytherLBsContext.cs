using System;
using Microsoft.EntityFrameworkCore;

namespace SpythereLBs;

public class SpythereLBsContext(DbContextOptions<SpythereLBsContext> options) : DbContext(options)
{

}
