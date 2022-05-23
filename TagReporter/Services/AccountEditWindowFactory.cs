using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using TagReporter.Contracts.Services;
using TagReporter.Models;
using TagReporter.ViewModels;
using TagReporter.Views;

namespace TagReporter.Services;

public class AccountEditWindowFactory: IAccountEditWindowFactory
{
    private readonly AccountEditViewModel _viewModel;

    public AccountEditWindowFactory(AccountEditViewModel viewModel)
    {
        _viewModel = viewModel;
    }

    public AccountEditWindow Create(EditMode mode, WstAccount? account)
    {
        var wnd = new AccountEditWindow(_viewModel);
        _viewModel.SetMode(mode, account);
        return wnd;
    }
}