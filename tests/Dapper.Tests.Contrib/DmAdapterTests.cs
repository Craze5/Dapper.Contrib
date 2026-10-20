using System.Text;
using Xunit;

namespace Dapper.Tests.Contrib
{
    /// <summary>
    /// Pure unit tests for the DM (Dameng) adapter SQL generation - no database server required.
    /// </summary>
    public class DmAdapterTests
    {
        [Fact]
        public void DmAdapterImplementsISqlAdapter()
        {
            Assert.IsAssignableFrom<ISqlAdapter>(new DmAdapter());
        }

        [Fact]
        public void AppendColumnNameWrapsColumnNameInDoubleQuotes()
        {
            var sb = new StringBuilder();
            new DmAdapter().AppendColumnName(sb, "Name");
            Assert.Equal("\"Name\"", sb.ToString());
        }

        [Fact]
        public void AppendColumnNameEqualsValueUsesDoubleQuotesAndColonParameter()
        {
            var sb = new StringBuilder();
            new DmAdapter().AppendColumnNameEqualsValue(sb, "Name");
            Assert.Equal("\"Name\" = :Name", sb.ToString());
        }

        [Fact]
        public void AppendColumnNameHandlesReservedKeywordOrder()
        {
            var sb = new StringBuilder();
            new DmAdapter().AppendColumnName(sb, "Order");
            Assert.Equal("\"Order\"", sb.ToString());
        }

        [Fact]
        public void AppendColumnNameEqualsValueHandlesReservedKeywordOrder()
        {
            var sb = new StringBuilder();
            new DmAdapter().AppendColumnNameEqualsValue(sb, "Order");
            Assert.Equal("\"Order\" = :Order", sb.ToString());
        }
    }
}
