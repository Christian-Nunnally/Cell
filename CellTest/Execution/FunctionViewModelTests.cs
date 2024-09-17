using Cell.Model;
using Cell.ViewModel.Execution;

namespace CellTest.Execution
{
    public class FunctionViewModelTests
    {
        [Fact]
        public void UserFriendlyCodeSet_UserFriendlyCodeReturned_CodeUnchanged()
        {
            var model = new PluginFunctionModel();
            var testing = new PluginFunction(model);
            var testCode = @"
var itemsToSearch = new List<TodoItem>();
itemsToSearch.Add(todoItem);
while (itemsToSearch.Any())
{
    var item = itemsToSearch.Last();
    itemsToSearch.AddRange(todo.Where(x => x.Parent == item.TaskID));
    var clone = (TodoItem)item.Clone();
    c.GetUserList<TodoItem>(Todo_B_I3.Text).Add(clone);
    todo.Remove(item);
    itemsToSearch.Remove(item);
}";
            testing.SetUserFriendlyCode(testCode, CellModel.Null, x => x, []);

            var result = testing.GetUserFriendlyCode(CellModel.Null, x => x, []);

            Assert.Equal(testCode, result);
        }

        [Fact]
        public void UserFriendlyCodeSetWithTabs_UserFriendlyCodeReturned_CodeNowContainsSpaces()
        {
            var model = new PluginFunctionModel();
            var testing = new PluginFunction(model);
            var testCode = "\treturn test;";
            testing.SetUserFriendlyCode(testCode, CellModel.Null, x => x, []);

            var result = testing.GetUserFriendlyCode(CellModel.Null, x => x, []);

            Assert.Equal("    return test;", result);
        }
    }
}
