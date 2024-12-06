using Cell.Core.Common;
using Cell.Core.Execution.Functions;
using Cell.Model;

namespace CellTest.Core.Execution
{
    public class FunctionViewModelTests
    {
        [Fact]
        public void UserFriendlyCodeSet_UserFriendlyCodeReturned_CodeUnchanged()
        {
            var model = new CellFunctionModel();
            var testing = new CellFunction(model, Logger.Null);
            var testCode = @"
var itemsToSearch = new List<UserItem>();
itemsToSearch.Add(todoItem);
while (itemsToSearch.Any())
{
    var item = itemsToSearch.Last();
    itemsToSearch.AddRange(todo.Where(x => x.Parent == item.TaskID));
    var clone = (UserItem)item.Clone();
    c.GetUserList<UserItem>(Todo_B_I3.Text).Add(clone);
    todo.Remove(item);
    itemsToSearch.Remove(item);
}";
            testing.SetUserFriendlyCode(testCode, CellModel.Null, []);

            var result = testing.GetUserFriendlyCode(CellModel.Null, []);

            Assert.Equal(testCode, result);
        }

        [Fact]
        public void UserFriendlyCodeSetWithTabs_UserFriendlyCodeReturned_CodeNowContainsSpaces()
        {
            var model = new CellFunctionModel();
            var testing = new CellFunction(model, Logger.Null);
            var testCode = "\treturn test;";
            testing.SetUserFriendlyCode(testCode, CellModel.Null, []);

            var result = testing.GetUserFriendlyCode(CellModel.Null, []);

            Assert.Equal("    return test;", result);
        }
    }
}
