// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ImportaGxP
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        string CaminhoSIAFWor;
        string CaminhoSIAFWde;
        List<string> QueryUpdate;
        public MainWindow()
        {
            this.InitializeComponent();

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed)
                    ListaPastas(drive.Name);
            }
            QueryUpdate = new();
        }

        private void ListaPastas(string drive)
        {
            DirectoryInfo DiscoLocal = new DirectoryInfo(drive);
            DirectoryInfo[] PastasSiaf = DiscoLocal.GetDirectories("Siafw" + "*.*");

            foreach (DirectoryInfo d in PastasSiaf)
            {
                ComboSiafwOrig.Items.Add(d.FullName + "\\SIAFW.FDB");
                ComboSiafwDest.Items.Add(d.FullName + "\\SIAFW.FDB");
            }
        }

        private void BotaoImportar_Click(object sender, RoutedEventArgs e)
        {
            List<string> abas = new();

            if ((bool)CBoxG1.IsChecked) abas.Add("G1");
            if ((bool)CBoxG2.IsChecked) abas.Add("G2");
            if ((bool)CBoxNF.IsChecked) abas.Add("NF");
            if ((bool)CBoxContas.IsChecked) abas.Add("RE");
            if ((bool)CBoxProd.IsChecked) abas.Add("PR");
            if ((bool)CBoxCli.IsChecked) abas.Add("CL");
            if ((bool)CBoxGmt.IsChecked) abas.Add("MR");
            if ((bool)CBoxPdv.IsChecked) abas.Add("MN");

            string andAbas = GeraStringAndAbas(abas);

            if(abas.Count == 0)
            {
                MostraAlertaSemAbas();
                return;
            }

            var grupoOri = ComboGrupoOrig.SelectedItem.ToString();
            grupoOri = grupoOri.Remove(grupoOri.IndexOf(' '));

            var grupoDes = ComboGrupoDest.SelectedItem.ToString();
            grupoDes = grupoDes.Remove(grupoDes.IndexOf(' '));

            var conexao = new ConexaoFirebird(CaminhoSIAFWor);
            var tbGxpOri = conexao.ExecutarSelect($"SELECT * FROM DSIAF051 WHERE GRU_USU = '{grupoOri}' {andAbas}");
            conexao = null;

            conexao = new ConexaoFirebird(CaminhoSIAFWde);
            var tabela = conexao.ExecutarSelect($"SELECT PROG_DESC FROM DSIAF051 WHERE GRU_USU = '{grupoDes}'");

            List<string> DescDest = new();
            for (int i = 0; i < tabela.Rows.Count; i++)
                DescDest.Add(tabela.Rows[i][0].ToString());

            QueryUpdate.Clear();

            for (int i = 0; i < tbGxpOri.Rows.Count; i++)
            {
                if (DescDest.Contains(tbGxpOri.Rows[i][1]))
                {
                    QueryUpdate.Add(@$"UPDATE DSIAF051 SET PROG_ACE = '{tbGxpOri.Rows[i][2]}', PROG_INC = '{tbGxpOri.Rows[i][3]}',
PROG_ALT = '{tbGxpOri.Rows[i][4]}', PROG_EXC = '{tbGxpOri.Rows[i][5]}', PROG_IMP = '{tbGxpOri.Rows[i][6]}' 
WHERE PROG_DESC = '{tbGxpOri.Rows[i][1]}' AND GRU_USU = '{grupoDes}'; ");
//                    Teste.Items.Add(tbGxpOri.Rows[i][1]);
                }  
            }

 //           TesteComando.Text = QueryUpdate.First();
            MostraConfirmacaoAtualiza();
        }

        private async void MostraConfirmacaoAtualiza()
        {
            DialogoAtualizacao.XamlRoot = this.Content.XamlRoot;
            await DialogoAtualizacao.ShowAsync();
        }

        string GeraStringAndAbas(List<string> abas)
        {
            if (abas.Count == 0) return null;

            string retorno = $"AND (PROG_MOD = '{abas.First()}'";

            foreach(var aba in abas.Skip(1))
            {
                retorno += $" OR PROG_MOD = '{aba}'";
            }
            retorno += ")";
            return retorno;
        }

        private void ComboSiafwOr_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CaminhoSIAFWor = ComboSiafwOrig.SelectedItem.ToString();

            var conexao = new ConexaoFirebird(CaminhoSIAFWor);
            var tabelaGrupos = conexao.ExecutarSelect("SELECT GRU_USU, GRU_DUSU FROM DSIAF053");

            ComboGrupoOrig.Items.Clear();

            for(int i = 0; i < tabelaGrupos.Rows.Count; i++)
            {
                ComboGrupoOrig.Items.Add($"{tabelaGrupos.Rows[i][0]} - {tabelaGrupos.Rows[i][1]}");
            }
        }

        private void ComboSiafwDe_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CaminhoSIAFWde = ComboSiafwDest.SelectedItem.ToString();

            var conexao = new ConexaoFirebird(CaminhoSIAFWde);
            var tabelaGrupos = conexao.ExecutarSelect("SELECT GRU_USU, GRU_DUSU FROM DSIAF053");

            ComboGrupoDest.Items.Clear();

            for (int i = 0; i < tabelaGrupos.Rows.Count; i++)
            {
                ComboGrupoDest.Items.Add($"{tabelaGrupos.Rows[i][0]} - {tabelaGrupos.Rows[i][1]}");
            }
        }

        private async void MostraAlertaSemAbas()
        {
            ContentDialog Dialog = new ContentDialog()
            {
                Title = "Atenção",
                Content = "Selecione ao menos uma aba",
                CloseButtonText = "Ok"
            };

            Dialog.XamlRoot = this.Content.XamlRoot;
            await Dialog.ShowAsync();
        }

        private async void MostraAlertaErroFirebird(string erro)
        {
            DialogoAtualizacao.Hide(); //Importante
            ContentDialog Dialog = new ContentDialog()
            {
                Title = "Erro durante atualização",
                Content = erro,
                CloseButtonText = "Ok"
            };

            Dialog.XamlRoot = this.Content.XamlRoot;
            await Dialog.ShowAsync();
        }

        private async void MostraAlertaAtualizacaoRealizada(string n)
        {
            DialogoAtualizacao.Hide();
            ContentDialog Dialog = new ContentDialog()
            {
                Title = "Atualização realizada",
                Content = $"{n} permissões foram atualizadas!",
                CloseButtonText = "Ok"
            };

            Dialog.XamlRoot = this.Content.XamlRoot;
            await Dialog.ShowAsync();
        }

        private void AtualizacaoConfirmada(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var conexao = new ConexaoFirebird(CaminhoSIAFWde);
            string retorno;
            int sum = 0;

            foreach(string comando in QueryUpdate)
            {
                retorno = conexao.ExecutarComando(comando);
                sum += int.Parse(retorno);

                if (IsDigitsOnly(retorno) == false)
                {
                    MostraAlertaErroFirebird(retorno);
                    return;
                }
            }

            retorno = sum.ToString();
            MostraAlertaAtualizacaoRealizada(retorno);
        }

        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }
    }
}
