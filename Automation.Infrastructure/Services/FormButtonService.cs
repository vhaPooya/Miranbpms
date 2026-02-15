using Automation.Core.Entities;
using Automation.Infrastructure.Data;
using Automation.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Automation.Infrastructure.Services;

public class FormButtonService : IFormButtonService
{
    private readonly AutomationDbContext _context;

    public FormButtonService(AutomationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FormButtonTypeDto>> GetPredefinedButtonTypesAsync()
    {
        return await _context.FormButtonTypes
            .OrderBy(bt => bt.DisplayOrder)
            .Select(bt => new FormButtonTypeDto
            {
                Id = bt.Id,
                ButtonCode = bt.ButtonCode,
                NameFa = bt.NameFa,
                NameEn = bt.NameEn,
                DefaultIcon = bt.DefaultIcon,
                DefaultColor = bt.DefaultColor,
                Category = bt.Category,
                ActionHandler = bt.ActionHandler,
                OpensModal = bt.OpensModal,
                ModalId = bt.ModalId,
                DisplayOrder = bt.DisplayOrder,
                Description = bt.Description
            })
            .ToListAsync();
    }

    public async Task<List<ButtonStylePresetDto>> GetStylePresetsAsync()
    {
        return await _context.ButtonStylePresets
            .OrderBy(sp => sp.DisplayOrder)
            .Select(sp => new ButtonStylePresetDto
            {
                Id = sp.Id,
                PresetName = sp.PresetName,
                Description = sp.Description,
                StyleDefinition = sp.StyleDefinition,
                IsSystem = sp.IsSystem,
                DisplayOrder = sp.DisplayOrder
            })
            .ToListAsync();
    }

    public async Task<FormButtonConfigDto> GetFormButtonConfigAsync(int formId)
    {
        var form = await _context.Forms
            .AsNoTracking()
            .Where(f => f.Id == formId)
            .Select(f => new { f.ButtonBarSettings })
            .FirstOrDefaultAsync();

        if (form == null)
            return new FormButtonConfigDto();

        var buttons = await _context.FormButtons
            .AsNoTracking()
            .Where(fb => fb.FormId == formId)
            .Include(fb => fb.FormButtonType)
            .Include(fb => fb.ButtonStylePreset)
            .OrderBy(fb => fb.DisplayOrder)
            .Select(fb => new FormButtonDto
            {
                Id = fb.Id,
                FormId = fb.FormId,
                FormButtonTypeId = fb.FormButtonTypeId,
                IsProcessServiceButton = fb.IsProcessServiceButton,
                CustomTitleFa = fb.CustomTitleFa,
                CustomTitleEn = fb.CustomTitleEn,
                DisplayOrder = fb.DisplayOrder,
                DisplayMode = fb.DisplayMode,
                CustomIcon = fb.CustomIcon,
                StyleSettings = fb.StyleSettings,
                ButtonStylePresetId = fb.ButtonStylePresetId,
                WorkflowServiceId = fb.WorkflowServiceId,
                InheritFromButtonTypeId = fb.InheritFromButtonTypeId,
                ExecutionTiming = fb.ExecutionTiming,
                ProcessServiceConfig = fb.ProcessServiceConfig,
                VisibilityCondition = fb.VisibilityCondition,
                RequiredPermission = fb.RequiredPermission,
                ConfirmationMessage = fb.ConfirmationMessage,
                IsEnabled = fb.IsEnabled,
                // Related entity data
                ButtonTypeName = fb.FormButtonType != null ? fb.FormButtonType.NameFa : null,
                ButtonTypeIcon = fb.FormButtonType != null ? fb.FormButtonType.DefaultIcon : null,
                ButtonTypeColor = fb.FormButtonType != null ? fb.FormButtonType.DefaultColor : null,
                ButtonTypeActionHandler = fb.FormButtonType != null ? fb.FormButtonType.ActionHandler : null,
                ButtonTypeOpensModal = fb.FormButtonType != null ? fb.FormButtonType.OpensModal : (bool?)null,
                ButtonTypeModalId = fb.FormButtonType != null ? fb.FormButtonType.ModalId : null,
                PresetName = fb.ButtonStylePreset != null ? fb.ButtonStylePreset.PresetName : null
            })
            .ToListAsync();

        return new FormButtonConfigDto
        {
            ButtonBarSettings = form.ButtonBarSettings,
            Buttons = buttons
        };
    }

    public async Task<bool> SaveFormButtonConfigAsync(int formId, FormButtonConfigDto config)
    {
        var form = await _context.Forms
            .Include(f => f.Buttons)
            .FirstOrDefaultAsync(f => f.Id == formId);

        if (form == null)
            return false;

        // Update button bar settings
        form.ButtonBarSettings = config.ButtonBarSettings;

        // Remove existing buttons
        if (form.Buttons.Any())
        {
            _context.FormButtons.RemoveRange(form.Buttons);
        }

        // Add new buttons
        foreach (var btnDto in config.Buttons)
        {
            var button = new FormButton
            {
                FormId = formId,
                FormButtonTypeId = btnDto.FormButtonTypeId,
                IsProcessServiceButton = btnDto.IsProcessServiceButton,
                CustomTitleFa = btnDto.CustomTitleFa,
                CustomTitleEn = btnDto.CustomTitleEn,
                DisplayOrder = btnDto.DisplayOrder,
                DisplayMode = btnDto.DisplayMode,
                CustomIcon = btnDto.CustomIcon,
                StyleSettings = btnDto.StyleSettings ?? "{}",
                ButtonStylePresetId = btnDto.ButtonStylePresetId,
                WorkflowServiceId = btnDto.WorkflowServiceId,
                InheritFromButtonTypeId = btnDto.InheritFromButtonTypeId,
                ExecutionTiming = btnDto.ExecutionTiming,
                ProcessServiceConfig = btnDto.ProcessServiceConfig,
                VisibilityCondition = btnDto.VisibilityCondition,
                RequiredPermission = btnDto.RequiredPermission,
                ConfirmationMessage = btnDto.ConfirmationMessage,
                IsEnabled = btnDto.IsEnabled,
                CreatedAt = DateTime.UtcNow
            };
            _context.FormButtons.Add(button);
        }

        await _context.SaveChangesAsync();
        return true;
    }
}


