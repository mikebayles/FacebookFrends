<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FaceBookFriends._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title></title>
    </head>
    <body>
        <form runat="server">
            <asp:Repeater runat="server" ID="Repeater1">
                 <ItemTemplate>
                    <p> <%# Container.DataItem %></p>           
                </ItemTemplate>
            </asp:Repeater>

            <asp:Button ID="btnSubmit" runat="server" Text="Refresh" OnClick="btnSubmit_Click" />
        </form>
    </body>
</html>