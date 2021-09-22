<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="Program4NONSQL.About" Async ="true"%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        <asp:Button ID="Button1" runat="server" Text="Load Data" Width="213px" OnClick="Button1_Click" OnClientClick="Button1_Click" />
    </h2>
    <h2>
        <asp:Button ID="Button2" runat="server" Text="Clear Data" OnClick="Button2_Click" />
    </h2>
    <p>Last name:<asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
    </p>
    <p>First name:<asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
    </p>
    <p>
        <asp:Button ID="Button3" runat="server" Text="Query" OnClick="Button3_Click" />
    </p>
    <p>
        <asp:Label ID="Label1" runat="server" Text="result"></asp:Label>
    </p>
    </asp:Content>
