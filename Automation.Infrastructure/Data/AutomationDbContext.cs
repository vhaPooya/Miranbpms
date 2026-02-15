using Automation.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Automation.Infrastructure.Data;

/// <summary>
/// Database context for Automation system
/// </summary>
public class AutomationDbContext : DbContext
{
    public AutomationDbContext(DbContextOptions<AutomationDbContext> options) : base(options)
    {
    }

    // Form Builder Entities
    public DbSet<Form> Forms { get; set; } = null!;
    public DbSet<FormField> FormFields { get; set; } = null!;
    public DbSet<FieldType> FieldTypes { get; set; } = null!;
    public DbSet<FormCategory> FormCategories { get; set; } = null!;
    public DbSet<FormScript> FormScripts { get; set; } = null!;
    public DbSet<FormStyle> FormStyles { get; set; } = null!;
    public DbSet<FormValidation> FormValidations { get; set; } = null!;
    public DbSet<ComboboxItem> ComboboxItems { get; set; } = null!;

    // New Entities
    public DbSet<ScalarFunction> ScalarFunctions { get; set; } = null!;
    public DbSet<FormStaticAsset> FormStaticAssets { get; set; } = null!;
    public DbSet<PrintTemplate> PrintTemplates { get; set; } = null!;
    
    // Identity/Core: use IdentityDbContext and CoreDbContext; entity types kept in model only for Document/Email/Fax FKs and Include().

    // Form Design & Data (candidates for Dapper/SP; high-volume runtime data)
    public DbSet<FormDesign> FormDesigns { get; set; } = null!;
    public DbSet<FormDataTable> FormDataTables { get; set; } = null!;

    // Form Action Buttons
    public DbSet<FormButtonType> FormButtonTypes { get; set; } = null!;
    public DbSet<FormButton> FormButtons { get; set; } = null!;
    public DbSet<ButtonStylePreset> ButtonStylePresets { get; set; } = null!;
    
    // Document Management
    public DbSet<DocumentAttachment> DocumentAttachments { get; set; } = null!;
    public DbSet<DocumentReferral> DocumentReferrals { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<DocumentTracking> DocumentTrackings { get; set; } = null!;
    public DbSet<IncomingDocument> IncomingDocuments { get; set; } = null!;
    public DbSet<OutgoingDocument> OutgoingDocuments { get; set; } = null!;
    public DbSet<DocumentRegister> DocumentRegisters { get; set; } = null!;
    
    // Document Metadata
    public DbSet<DocumentType> DocumentTypes { get; set; } = null!;
    public DbSet<Classification> Classifications { get; set; } = null!;
    public DbSet<ReceiptMethod> ReceiptMethods { get; set; } = null!;
    public DbSet<Urgency> Urgencies { get; set; } = null!;
    
    // Document Extensions
    public DbSet<DocumentCopyRecipient> DocumentCopyRecipients { get; set; } = null!;
    public DbSet<DocumentKeyword> DocumentKeywords { get; set; } = null!;
    public DbSet<ReservedDocumentNumber> ReservedDocumentNumbers { get; set; } = null!;
    
// Email & Fax
    public DbSet<EmailServiceSettings> EmailServiceSettings { get; set; } = null!;
    public DbSet<EmailMessage> EmailMessages { get; set; } = null!;
    public DbSet<FaxServiceSettings> FaxServiceSettings { get; set; } = null!;
    public DbSet<FaxMessage> FaxMessages { get; set; } = null!;
    
    // Workflow Engine Entities
    public DbSet<Workflow> Workflows { get; set; } = null!;
    public DbSet<WorkflowCategory> WorkflowCategories { get; set; } = null!;
    public DbSet<WorkflowVersion> WorkflowVersions { get; set; } = null!;
    public DbSet<WorkflowNode> WorkflowNodes { get; set; } = null!;
    public DbSet<WorkflowConnection> WorkflowConnections { get; set; } = null!;
    public DbSet<WorkflowVariable> WorkflowVariables { get; set; } = null!;
    // Workflow execution (WorkflowInstance, WorkflowInstanceVariable, WorkflowInstanceTransition): use IDapperService/SPs only.

    // Report Builder Entities
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<ReportCategory> ReportCategories { get; set; } = null!;
    public DbSet<ReportField> ReportFields { get; set; } = null!;
    public DbSet<ReportFilter> ReportFilters { get; set; } = null!;
    public DbSet<ReportGroup> ReportGroups { get; set; } = null!;
    public DbSet<ReportSort> ReportSorts { get; set; } = null!;
    public DbSet<ReportOutput> ReportOutputs { get; set; } = null!;

    // Numbering Rules
    public DbSet<NumberingRule> NumberingRules { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        ConfigureFormCategory(modelBuilder);
        ConfigureFieldType(modelBuilder);
        ConfigureForm(modelBuilder);
        ConfigureFormField(modelBuilder);
        ConfigureFormScript(modelBuilder);
        ConfigureFormStyle(modelBuilder);
        ConfigureFormValidation(modelBuilder);
        ConfigureComboboxItem(modelBuilder);
        ConfigureScalarFunction(modelBuilder);
        ConfigureFormStaticAsset(modelBuilder);
        ConfigurePrintTemplate(modelBuilder);
        
        // System Entities
        ConfigureUser(modelBuilder);
        ConfigureRole(modelBuilder);
        ConfigurePosition(modelBuilder);
        ConfigureUserPosition(modelBuilder);
        ConfigureRolePosition(modelBuilder);
        ConfigureUserRole(modelBuilder);
        ConfigureUserSignature(modelBuilder);
        ConfigurePermission(modelBuilder);
        ConfigureRolePermission(modelBuilder);
        ConfigureDepartment(modelBuilder);
        ConfigureOrganization(modelBuilder);
        
        // Form Design & Data
        ConfigureFormDesign(modelBuilder);
        ConfigureFormDataTable(modelBuilder);

        // Form Action Buttons
        ConfigureFormButtonType(modelBuilder);
        ConfigureFormButton(modelBuilder);
        ConfigureButtonStylePreset(modelBuilder);

        // System Settings
        ConfigureSystemSetting(modelBuilder);
        ConfigureConnectionString(modelBuilder);
        ConfigureWebService(modelBuilder);
        ConfigureActionType(modelBuilder);
        ConfigurePriority(modelBuilder);
        ConfigureStatus(modelBuilder);
        ConfigureConfidentialityLevel(modelBuilder);
        ConfigureTheme(modelBuilder);
        
        // Permission & Group Management
        ConfigureGroup(modelBuilder);
        ConfigureGroupMember(modelBuilder);
        ConfigureGroupPermission(modelBuilder);
        ConfigurePermissionGroup(modelBuilder);
        ConfigureRoleGroup(modelBuilder);

        // Document Management
        ConfigureDocumentAttachment(modelBuilder);
        ConfigureDocumentReferral(modelBuilder);
        ConfigureDocument(modelBuilder);
        ConfigureDocumentTracking(modelBuilder);
        ConfigureIncomingDocument(modelBuilder);
        ConfigureOutgoingDocument(modelBuilder);
        ConfigureDocumentRegister(modelBuilder);
        
        // Document Extensions
        ConfigureDocumentCopyRecipient(modelBuilder);
        ConfigureDocumentKeyword(modelBuilder);
        ConfigureReservedDocumentNumber(modelBuilder);
        
// Email & Fax
        ConfigureEmailServiceSettings(modelBuilder);
        ConfigureEmailMessage(modelBuilder);
        ConfigureFaxServiceSettings(modelBuilder);
        ConfigureFaxMessage(modelBuilder);
        
        // Workflow Engine
        ConfigureWorkflow(modelBuilder);
        ConfigureWorkflowCategory(modelBuilder);
        ConfigureWorkflowVersion(modelBuilder);
        ConfigureWorkflowNode(modelBuilder);
        ConfigureWorkflowConnection(modelBuilder);
        ConfigureWorkflowVariable(modelBuilder);
        ConfigureWorkflowInstance(modelBuilder);
        ConfigureWorkflowInstanceVariable(modelBuilder);
        ConfigureWorkflowInstanceTransition(modelBuilder);

        // Report Builder
        ConfigureReport(modelBuilder);
        ConfigureReportCategory(modelBuilder);
        ConfigureReportField(modelBuilder);
        ConfigureReportFilter(modelBuilder);
        ConfigureReportGroup(modelBuilder);
        ConfigureReportSort(modelBuilder);
        ConfigureReportOutput(modelBuilder);

        // Numbering Rules
        modelBuilder.Entity<NumberingRule>(entity =>
        {
            entity.ToTable("NumberingRules");
            entity.HasKey(e => e.Id);
        });

        // Global query filter for soft delete
        modelBuilder.Entity<Form>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FormField>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FieldType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FormCategory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FormScript>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FormStyle>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FormValidation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ComboboxItem>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ScalarFunction>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FormStaticAsset>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PrintTemplate>().HasQueryFilter(e => !e.IsDeleted);
        
        // System Entities
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Position>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserPosition>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RolePosition>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserRole>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserSignature>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RolePermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Department>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Organization>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Group>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<GroupMember>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<GroupPermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PermissionGroup>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DocumentAttachment>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DocumentReferral>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ActionType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Document>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DocumentTracking>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<IncomingDocument>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<OutgoingDocument>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DocumentRegister>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DocumentCopyRecipient>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DocumentKeyword>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EmailServiceSettings>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<EmailMessage>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FaxServiceSettings>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FaxMessage>().HasQueryFilter(e => !e.IsDeleted);
        // Workflow removed (to be redesigned)
        
        // Form Design & Data
        modelBuilder.Entity<FormDesign>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FormDataTable>().HasQueryFilter(e => !e.IsDeleted);

        // Form Action Buttons
        modelBuilder.Entity<FormButtonType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<FormButton>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ButtonStylePreset>().HasQueryFilter(e => !e.IsDeleted);

        // System Settings
        modelBuilder.Entity<SystemSetting>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ConnectionString>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WebService>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ActionType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Priority>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Status>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ConfidentialityLevel>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Theme>().HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.Entity<ReservedDocumentNumber>().HasQueryFilter(r => !r.IsDeleted);

        modelBuilder.Entity<Secretariat>().HasQueryFilter(s => !s.Organization.IsDeleted);

        modelBuilder.Entity<RoleGroup>()
            .HasQueryFilter(rg => !rg.Group.IsDeleted);

        // Workflow Engine (definition only; execution via Dapper/SP)
        modelBuilder.Entity<Workflow>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WorkflowCategory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WorkflowVersion>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WorkflowNode>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WorkflowConnection>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WorkflowVariable>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WorkflowInstance>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WorkflowInstanceVariable>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<WorkflowInstanceTransition>().HasQueryFilter(e => !e.IsDeleted);

        // Report Builder
        modelBuilder.Entity<Report>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReportCategory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReportField>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReportFilter>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReportGroup>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReportSort>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ReportOutput>().HasQueryFilter(e => !e.IsDeleted);

        // Numbering Rules
        modelBuilder.Entity<NumberingRule>().HasQueryFilter(e => !e.IsDeleted);

    }

    // System Entity Configurations
    private void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Mobile).HasMaxLength(20);
            
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.RoleCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RoleName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.RoleCode).IsUnique();
            
            entity.HasOne(e => e.ParentRole)
                .WithMany(r => r.ChildRoles)
                .HasForeignKey(e => e.ParentRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.PersonnelUser)
                .WithMany()
                .HasForeignKey(e => e.PersonnelUserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Roles)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.CorrespondenceTitle).HasMaxLength(200);
            entity.Property(e => e.DisplayTitle).HasMaxLength(200);
        });
    }

    private void ConfigurePosition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Position>(entity =>
        {
            entity.ToTable("Positions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PositionCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PositionName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => e.PositionCode).IsUnique();

            entity.HasOne(e => e.ParentPosition)
                .WithMany(p => p.ChildPositions)
                .HasForeignKey(e => e.ParentPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureUserPosition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPosition>(entity =>
        {
            entity.ToTable("UserPositions");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.UserId, e.PositionId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserPositions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Position)
                .WithMany(p => p.UserPositions)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureRolePosition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePosition>(entity =>
        {
            entity.ToTable("RolePositions");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.RoleId, e.PositionId }).IsUnique();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePositions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Position)
                .WithMany(p => p.RolePositions)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureUserRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureUserSignature(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSignature>(entity =>
        {
            entity.ToTable("UserSignatures");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SignatureTitle).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SignaturePath).HasMaxLength(1000).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserSignatures)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.IsDefault });
        });
    }

    private void ConfigurePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.PermissionCode).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PermissionName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(100);
            
            entity.HasIndex(e => e.PermissionCode).IsUnique();
            
            entity.HasOne(e => e.PermissionGroup)
                .WithMany(g => g.Permissions)
                .HasForeignKey(e => e.PermissionGroupId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureRolePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => e.Id);
            
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();
            
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureDepartment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.DepartmentCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DepartmentName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.DepartmentCode).IsUnique();
            
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Departments)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Self-referencing for department hierarchy
            entity.HasOne(e => e.ParentDepartment)
                .WithMany(d => d.ChildDepartments)
                .HasForeignKey(e => e.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("Organizations");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.OrganizationCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OrganizationName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.OrganizationCode).IsUnique();
        });
    }
    
    private void ConfigureFormDesign(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormDesign>(entity =>
        {
            entity.ToTable("FormDesigns");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DesignData).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.CustomScripts).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CustomStyles).HasColumnType("nvarchar(max)");
            entity.Property(e => e.FormSettings).HasColumnType("nvarchar(max)");
            entity.HasOne(e => e.Form)
                .WithMany()
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.FormId, e.Version });
        });
    }

    private void ConfigureFormDataTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormDataTable>(entity =>
        {
            entity.ToTable("FormDataTables");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TableName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TableSchema).HasColumnType("nvarchar(max)");
            entity.HasIndex(e => e.TableName).IsUnique();
            entity.HasOne(e => e.Form)
                .WithMany()
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureFormButtonType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormButtonType>(entity =>
        {
            entity.ToTable("FormButtonTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ButtonCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NameFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.DefaultIcon).HasMaxLength(100);
            entity.Property(e => e.DefaultColor).HasMaxLength(50);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.ActionHandler).HasMaxLength(200);
            entity.Property(e => e.ModalId).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.ButtonCode).IsUnique();
        });
    }

    private void ConfigureFormButton(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormButton>(entity =>
        {
            entity.ToTable("FormButtons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomTitleFa).HasMaxLength(200);
            entity.Property(e => e.CustomTitleEn).HasMaxLength(200);
            entity.Property(e => e.DisplayMode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CustomIcon).HasMaxLength(100);
            entity.Property(e => e.StyleSettings).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ExecutionTiming).HasMaxLength(20);
            entity.Property(e => e.ProcessServiceConfig).HasColumnType("nvarchar(max)");
            entity.Property(e => e.VisibilityCondition).HasMaxLength(2000);
            entity.Property(e => e.RequiredPermission).HasMaxLength(100);
            entity.Property(e => e.ConfirmationMessage).HasMaxLength(500);

            entity.HasOne(e => e.Form)
                .WithMany(f => f.Buttons)
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FormButtonType)
                .WithMany()
                .HasForeignKey(e => e.FormButtonTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ButtonStylePreset)
                .WithMany()
                .HasForeignKey(e => e.ButtonStylePresetId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.InheritFromButtonType)
                .WithMany()
                .HasForeignKey(e => e.InheritFromButtonTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.FormId, e.DisplayOrder });
        });
    }

    private void ConfigureButtonStylePreset(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ButtonStylePreset>(entity =>
        {
            entity.ToTable("ButtonStylePresets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PresetName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.StyleDefinition).HasColumnType("nvarchar(max)");
        });
    }

    private void ConfigureSystemSetting(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.ToTable("SystemSettings");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.SettingKey).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SettingValue).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.SettingType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.SettingKey).IsUnique();
        });
    }
    
    private void ConfigureConnectionString(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConnectionString>(entity =>
        {
            entity.ToTable("ConnectionStrings");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ConnectionStringValue).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.DatabaseType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.Name).IsUnique();
        });
    }
    
    private void ConfigureWebService(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WebService>(entity =>
        {
            entity.ToTable("WebServices");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ServiceName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ServiceUrl).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.ServiceType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.HttpMethod).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Headers).HasColumnType("nvarchar(max)");
            entity.Property(e => e.AuthType).HasMaxLength(50);
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.ServiceName).IsUnique();
        });
    }
    
    private void ConfigureActionType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActionType>(entity =>
        {
            entity.ToTable("ActionTypes");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ActionCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ActionNameFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ActionNameEn).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.ActionCode).IsUnique();
        });
    }
    
    private void ConfigurePriority(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Priority>(entity =>
        {
            entity.ToTable("Priorities");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.PriorityCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PriorityName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.PriorityCode).IsUnique();
        });
    }
    
    private void ConfigureStatus(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Status>(entity =>
        {
            entity.ToTable("Statuses");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.StatusCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.StatusName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.StatusCode).IsUnique();
        });
    }
    
    private void ConfigureConfidentialityLevel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConfidentialityLevel>(entity =>
        {
            entity.ToTable("ConfidentialityLevels");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.LevelCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LevelName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.LevelCode).IsUnique();
        });
    }
    
    private void ConfigureTheme(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Theme>(entity =>
        {
            entity.ToTable("Themes");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ThemeName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PrimaryColor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SecondaryColor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.BackgroundColor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.TextColor).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FontFamily).HasMaxLength(200);
            entity.Property(e => e.CustomCss).HasColumnType("nvarchar(max)");
            
            entity.HasIndex(e => e.ThemeName).IsUnique();
        });
    }

    private void ConfigureFormCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormCategory>(entity =>
        {
            entity.ToTable("FormCategories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NameFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(100);

            entity.HasIndex(e => e.Code).IsUnique();

            // Self-referencing relationship for hierarchy
            entity.HasOne(e => e.ParentCategory)
                .WithMany(e => e.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureFieldType(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FieldType>(entity =>
        {
            entity.ToTable("FieldTypes");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DisplayNameFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DisplayNameEn).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DefaultProperties).HasColumnType("nvarchar(max)");
            entity.Property(e => e.DefaultStyles).HasColumnType("nvarchar(max)");
            entity.Property(e => e.HtmlTemplate).HasColumnType("nvarchar(max)");
            entity.Property(e => e.RazorTemplate).HasColumnType("nvarchar(max)");

            entity.HasIndex(e => e.Name).IsUnique();
        });
    }

    private void ConfigureForm(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Form>(entity =>
        {
            entity.ToTable("Forms");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FormCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NameFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.NumberingRule).HasMaxLength(200);
            entity.Property(e => e.NumberingPrefix).HasMaxLength(50);
            entity.Property(e => e.DatabaseTableName).HasMaxLength(200);
            entity.Property(e => e.CustomStyles).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CustomScripts).HasColumnType("nvarchar(max)");
            entity.Property(e => e.BootstrapSettings).HasColumnType("nvarchar(max)");
            entity.Property(e => e.FormSettings).HasColumnType("nvarchar(max)");
            entity.Property(e => e.DesignData).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ButtonBarSettings).HasColumnType("nvarchar(max)");

            entity.HasIndex(e => e.FormCode).IsUnique();

            entity.HasOne(e => e.Category)
                .WithMany(e => e.Forms)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureFormField(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormField>(entity =>
        {
            entity.ToTable("FormFields");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FieldKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.LabelFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.LabelEn).HasMaxLength(200);
            entity.Property(e => e.Placeholder).HasMaxLength(500);
            entity.Property(e => e.HelpText).HasMaxLength(1000);
            entity.Property(e => e.DefaultValue).HasMaxLength(1000);
            entity.Property(e => e.DatabaseColumnName).HasMaxLength(200);
            entity.Property(e => e.DatabaseColumnType).HasMaxLength(100);
            entity.Property(e => e.Properties).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Styles).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CssClasses).HasMaxLength(500);
            entity.Property(e => e.InlineStyles).HasMaxLength(2000);
            entity.Property(e => e.Events).HasColumnType("nvarchar(max)");
            entity.Property(e => e.RenderCondition).HasMaxLength(2000);
            entity.Property(e => e.Options).HasColumnType("nvarchar(max)");
            entity.Property(e => e.DataSourceUrl).HasMaxLength(500);

            entity.HasIndex(e => new { e.FormId, e.FieldKey }).IsUnique();

            entity.HasOne(e => e.Form)
                .WithMany(e => e.Fields)
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FieldType)
                .WithMany(e => e.FormFields)
                .HasForeignKey(e => e.FieldTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing for hierarchy
            entity.HasOne(e => e.ParentField)
                .WithMany(e => e.ChildFields)
                .HasForeignKey(e => e.ParentFieldId)
                .OnDelete(DeleteBehavior.Restrict);

            // New properties
            entity.Property(e => e.TooltipText).HasMaxLength(500);
            entity.Property(e => e.CustomCss).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CustomJs).HasColumnType("nvarchar(max)");

            // Default function relationship
            entity.HasOne(e => e.DefaultFunction)
                .WithMany(e => e.FormFields)
                .HasForeignKey(e => e.DefaultFunctionId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureFormScript(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormScript>(entity =>
        {
            entity.ToTable("FormScripts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Content).HasColumnType("nvarchar(max)").IsRequired();

            entity.HasOne(e => e.Form)
                .WithMany(e => e.Scripts)
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureFormStyle(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormStyle>(entity =>
        {
            entity.ToTable("FormStyles");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Selector).HasMaxLength(500);
            entity.Property(e => e.Content).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.ThemeName).HasMaxLength(100);
            entity.Property(e => e.MediaQuery).HasMaxLength(500);

            entity.HasOne(e => e.Form)
                .WithMany(e => e.Styles)
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureFormValidation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormValidation>(entity =>
        {
            entity.ToTable("FormValidations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Value).HasMaxLength(500);
            entity.Property(e => e.MinValue).HasMaxLength(100);
            entity.Property(e => e.MaxValue).HasMaxLength(100);
            entity.Property(e => e.Pattern).HasMaxLength(1000);
            entity.Property(e => e.ErrorMessageFa).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ErrorMessageEn).HasMaxLength(500);
            entity.Property(e => e.CustomScript).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ApplyCondition).HasMaxLength(1000);

            entity.HasOne(e => e.Form)
                .WithMany(e => e.Validations)
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FormField)
                .WithMany(e => e.Validations)
                .HasForeignKey(e => e.FormFieldId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureComboboxItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ComboboxItem>(entity =>
        {
            entity.ToTable("ComboboxItems");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TitleItem).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ValueItem).HasMaxLength(100);
            entity.Property(e => e.GroupCode).HasMaxLength(50).IsRequired();

            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureScalarFunction(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScalarFunction>(entity =>
        {
            entity.ToTable("ScalarFunctions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DisplayNameEn).HasMaxLength(200);
            entity.Property(e => e.FunctionBody).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.Parameters).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => e.Name).IsUnique();
        });
    }

    private void ConfigureFormStaticAsset(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FormStaticAsset>(entity =>
        {
            entity.ToTable("FormStaticAssets");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.EncryptedPath).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(100);
            entity.Property(e => e.Caption).HasMaxLength(500);
            entity.Property(e => e.Metadata).HasColumnType("nvarchar(max)");

            entity.HasOne(e => e.Form)
                .WithMany()
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FormField)
                .WithMany(e => e.StaticAssets)
                .HasForeignKey(e => e.FormFieldId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigurePrintTemplate(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrintTemplate>(entity =>
        {
            entity.ToTable("PrintTemplates");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.HtmlContent).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.CssStyles).HasColumnType("nvarchar(max)");
            entity.Property(e => e.HeaderHtml).HasColumnType("nvarchar(max)");
            entity.Property(e => e.FooterHtml).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Margins).HasMaxLength(500);
            entity.Property(e => e.CustomWidth).HasMaxLength(50);
            entity.Property(e => e.CustomHeight).HasMaxLength(50);
            entity.Property(e => e.FilePath).HasMaxLength(1000);

            entity.HasOne(e => e.Form)
                .WithMany()
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureGroup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("Groups");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.GroupCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.GroupName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => e.GroupCode).IsUnique();

            entity.HasOne(e => e.ParentGroup)
                .WithMany(g => g.ChildGroups)
                .HasForeignKey(e => e.ParentGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureGroupMember(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.ToTable("GroupMembers");
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.Notes).HasMaxLength(1000);
            
            // ?? ????? ????????? ?? ??? ?? ?? ???? ????
            entity.HasIndex(e => new { e.GroupId, e.UserId }).IsUnique();
        });
    }

    private void ConfigureGroupPermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GroupPermission>(entity =>
        {
            entity.ToTable("GroupPermissions");
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Group)
                .WithMany(g => g.GroupPermissions)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.GroupPermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // ?? ???? ????????? ?? ??? ?? ???? ?? ????? ????
            entity.HasIndex(e => new { e.GroupId, e.PermissionId }).IsUnique();
        });
    }

    private void ConfigureRoleGroup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RoleGroup>(entity =>
        {
            entity.ToTable("RoleGroups");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RoleGroups)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Group)
                .WithMany(g => g.RoleGroups)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // ?? ??? ????????? ?? ??? ?? ?? ???? ????
            entity.HasIndex(e => new { e.RoleId, e.GroupId }).IsUnique();
        });
    }

    private void ConfigurePermissionGroup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PermissionGroup>(entity =>
        {
            entity.ToTable("PermissionGroups");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.GroupCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.GroupName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasIndex(e => e.GroupCode).IsUnique();
            
            // Self-referencing relationship for hierarchy
            entity.HasOne(e => e.ParentGroup)
                .WithMany(g => g.ChildGroups)
                .HasForeignKey(e => e.ParentGroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureDocumentAttachment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentAttachment>(entity =>
        {
            entity.ToTable("DocumentAttachments");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.AttachmentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.AttachmentCategory).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
            entity.Property(e => e.MimeType).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            
            entity.HasOne(e => e.Form)
                .WithMany()
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ReferencedForm)
                .WithMany()
                .HasForeignKey(e => e.ReferencedFormId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.UploadedByUser)
                .WithMany()
                .HasForeignKey(e => e.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureDocumentReferral(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentReferral>(entity =>
        {
            entity.ToTable("DocumentReferrals");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(2000);
            
            entity.HasOne(e => e.Document)
                .WithMany(d => d.Referrals)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Form)
                .WithMany()
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ActionType)
                .WithMany(a => a.DocumentReferrals)
                .HasForeignKey(e => e.ActionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.ReferredToUser)
                .WithMany()
                .HasForeignKey(e => e.ReferredToUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.ReferredToRole)
                .WithMany()
                .HasForeignKey(e => e.ReferredToRoleId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.ReferredToDepartment)
                .WithMany()
                .HasForeignKey(e => e.ReferredToDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.ReferredToGroup)
                .WithMany()
                .HasForeignKey(e => e.ReferredToGroupId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.ReferredByUser)
                .WithMany()
                .HasForeignKey(e => e.ReferredByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureDocument(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.DocumentNumber).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DocumentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Content).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            
            entity.HasIndex(e => e.DocumentNumber).IsUnique();
            entity.HasIndex(e => new { e.FormId, e.CreatedDateTime });
            
            entity.HasOne(e => e.Form)
                .WithMany()
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.SignedByUser)
                .WithMany()
                .HasForeignKey(e => e.SignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.ConfidentialityLevel)
                .WithMany()
                .HasForeignKey(e => e.ConfidentialityLevelId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Priority)
                .WithMany()
                .HasForeignKey(e => e.PriorityId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureDocumentTracking(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentTracking>(entity =>
        {
            entity.ToTable("DocumentTrackings");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ActionType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(2000);
            
            entity.HasIndex(e => new { e.DocumentId, e.ActionDateTime });
            
            entity.HasOne(e => e.Document)
                .WithMany(d => d.TrackingHistory)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Referral)
                .WithMany()
                .HasForeignKey(e => e.ReferralId)
                .OnDelete(DeleteBehavior.SetNull);
            
        });
    }

    private void ConfigureIncomingDocument(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IncomingDocument>(entity =>
        {
            entity.ToTable("IncomingDocuments");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.IncomingNumber).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SenderName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.SenderNumber).HasMaxLength(100);
            entity.Property(e => e.ReceiptMethod).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ScannedFilePath).HasMaxLength(1000);
            entity.Property(e => e.SenderEmail).HasMaxLength(255);
            entity.Property(e => e.InitialNumber).HasMaxLength(100);
            entity.Property(e => e.SenderAddress).HasMaxLength(1000);
            entity.Property(e => e.SenderMobile).HasMaxLength(20);
            
            entity.HasIndex(e => e.IncomingNumber).IsUnique();
            entity.HasIndex(e => new { e.IncomingDate, e.IncomingNumber });
            
            entity.HasOne(e => e.Document)
                .WithOne()
                .HasForeignKey<IncomingDocument>(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.RegisteredByUser)
                .WithMany()
                .HasForeignKey(e => e.RegisteredByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.DocumentType)
                .WithMany()
                .HasForeignKey(e => e.DocumentTypeId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Classification)
                .WithMany()
                .HasForeignKey(e => e.ClassificationId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.ReceiptMethodEntity)
                .WithMany()
                .HasForeignKey(e => e.ReceiptMethodId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Urgency)
                .WithMany()
                .HasForeignKey(e => e.UrgencyId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Secretariat)
                .WithMany()
                .HasForeignKey(e => e.SecretariatId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureOutgoingDocument(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutgoingDocument>(entity =>
        {
            entity.ToTable("OutgoingDocuments");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.OutgoingNumber).HasMaxLength(100).IsRequired();
            entity.Property(e => e.RecipientName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.RecipientAddress).HasMaxLength(1000);
            entity.Property(e => e.DeliveryMethod).HasMaxLength(50).IsRequired();
            entity.Property(e => e.InitialNumber).HasMaxLength(100);
            
            entity.HasIndex(e => e.OutgoingNumber).IsUnique();
            entity.HasIndex(e => new { e.OutgoingDate, e.OutgoingNumber });
            
            entity.HasOne(e => e.Document)
                .WithOne()
                .HasForeignKey<OutgoingDocument>(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.RegisteredByUser)
                .WithMany()
                .HasForeignKey(e => e.RegisteredByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.DocumentType)
                .WithMany()
                .HasForeignKey(e => e.DocumentTypeId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Classification)
                .WithMany()
                .HasForeignKey(e => e.ClassificationId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Urgency)
                .WithMany()
                .HasForeignKey(e => e.UrgencyId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.Secretariat)
                .WithMany()
                .HasForeignKey(e => e.SecretariatId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureDocumentRegister(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentRegister>(entity =>
        {
            entity.ToTable("DocumentRegisters");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.RegisterType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Prefix).HasMaxLength(10);
            entity.Property(e => e.NumberFormat).HasMaxLength(100).IsRequired();
            
            entity.HasIndex(e => new { e.RegisterType, e.Year, e.OrganizationId, e.DepartmentId }).IsUnique();
            
            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureDocumentCopyRecipient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentCopyRecipient>(entity =>
        {
            entity.ToTable("DocumentCopyRecipients");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.RecipientName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ActionType).HasMaxLength(50).IsRequired();
            
            entity.HasOne(e => e.Document)
                .WithMany(d => d.CopyRecipients)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureDocumentKeyword(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentKeyword>(entity =>
        {
            entity.ToTable("DocumentKeywords");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Keyword).HasMaxLength(200).IsRequired();
            
            entity.HasOne(e => e.Document)
                .WithMany(d => d.Keywords)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureReservedDocumentNumber(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReservedDocumentNumber>(entity =>
        {
            entity.ToTable("ReservedDocumentNumbers");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ReservedNumber).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DocumentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            
            entity.HasOne(e => e.Secretariat)
                .WithMany()
                .HasForeignKey(e => e.SecretariatId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.ReservedByUser)
                .WithMany()
                .HasForeignKey(e => e.ReservedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.UsedByDocument)
                .WithMany()
                .HasForeignKey(e => e.UsedByDocumentId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasIndex(e => new { e.ReservedNumber, e.SecretariatId, e.DocumentType, e.IsUsed });
        });
    }

    private void ConfigureEmailServiceSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailServiceSettings>(entity =>
        {
            entity.ToTable("EmailServiceSettings");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SmtpServer).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Password).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FromEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FromName).HasMaxLength(200);
            entity.Property(e => e.ImapServer).HasMaxLength(500);
            entity.Property(e => e.Pop3Server).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
            
            entity.HasOne(e => e.Secretariat)
                .WithMany()
                .HasForeignKey(e => e.SecretariatId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureEmailMessage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailMessage>(entity =>
        {
            entity.ToTable("EmailMessages");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.MessageType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FromEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FromName).HasMaxLength(200);
            entity.Property(e => e.ToEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ToName).HasMaxLength(200);
            entity.Property(e => e.Cc).HasMaxLength(1000);
            entity.Property(e => e.Bcc).HasMaxLength(1000);
            entity.Property(e => e.Subject).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Body).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ServerMessageId).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            
            entity.HasOne(e => e.Document)
                .WithMany()
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.EmailServiceSettings)
                .WithMany()
                .HasForeignKey(e => e.EmailServiceSettingsId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureFaxServiceSettings(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FaxServiceSettings>(entity =>
        {
            entity.ToTable("FaxServiceSettings");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.GatewayType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.GatewayUrl).HasMaxLength(1000);
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.Username).HasMaxLength(200);
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.DefaultFaxNumber).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            
            entity.HasOne(e => e.Secretariat)
                .WithMany()
                .HasForeignKey(e => e.SecretariatId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

private void ConfigureFaxMessage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FaxMessage>(entity =>
        {
            entity.ToTable("FaxMessages");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.MessageType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FromFaxNumber).HasMaxLength(50);
            entity.Property(e => e.FromName).HasMaxLength(200);
            entity.Property(e => e.ToFaxNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ToName).HasMaxLength(200);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.GatewayMessageId).HasMaxLength(500);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            
            entity.HasOne(e => e.Document)
                .WithMany()
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.HasOne(e => e.FaxServiceSettings)
                .WithMany()
                .HasForeignKey(e => e.FaxServiceSettingsId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    // Workflow Engine Configuration
    private void ConfigureWorkflow(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.ToTable("Workflows");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.WorkflowCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NameFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DesignData).HasColumnType("nvarchar(max)");

            entity.HasIndex(e => e.WorkflowCode).IsUnique();

            entity.HasOne(e => e.Category)
                .WithMany(e => e.Workflows)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Form)
                .WithMany()
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureWorkflowCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowCategory>(entity =>
        {
            entity.ToTable("WorkflowCategories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NameFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(100);

            entity.HasIndex(e => e.Code).IsUnique();

            entity.HasOne(e => e.ParentCategory)
                .WithMany(e => e.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureWorkflowVersion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowVersion>(entity =>
        {
            entity.ToTable("WorkflowVersions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DesignData).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.CustomScripts).HasColumnType("nvarchar(max)");
            entity.Property(e => e.FormSettings).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.Workflow)
                .WithMany(e => e.Versions)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureWorkflowNode(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowNode>(entity =>
        {
            entity.ToTable("WorkflowNodes");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TitleFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TitleEn).HasMaxLength(200);
            entity.Property(e => e.Settings).HasColumnType("nvarchar(max)");
            entity.Property(e => e.CustomScripts).HasColumnType("nvarchar(max)");
            entity.Property(e => e.VisibilityCondition).HasMaxLength(2000);

            entity.HasOne(e => e.Workflow)
                .WithMany(e => e.Nodes)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureWorkflowConnection(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowConnection>(entity =>
        {
            entity.ToTable("WorkflowConnections");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Label).HasMaxLength(200);
            entity.Property(e => e.Condition).HasMaxLength(2000);

            entity.HasOne(e => e.Workflow)
                .WithMany()
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SourceNode)
                .WithMany(e => e.OutgoingConnections)
                .HasForeignKey(e => e.SourceNodeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasPrincipalKey(e => e.Id);

            entity.HasOne(e => e.TargetNode)
                .WithMany(e => e.IncomingConnections)
                .HasForeignKey(e => e.TargetNodeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasPrincipalKey(e => e.Id);
        });
    }

    private void ConfigureWorkflowVariable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowVariable>(entity =>
        {
            entity.ToTable("WorkflowVariables");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DefaultValue).HasMaxLength(1000);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.Workflow)
                .WithMany(e => e.Variables)
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureWorkflowInstance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowInstance>(entity =>
        {
            entity.ToTable("WorkflowInstances");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.State).HasConversion<string>();
            entity.Property(e => e.StartedAt).IsRequired();
            entity.Property(e => e.CompletedAt).IsRequired(false);
            entity.HasOne(e => e.Workflow)
                .WithMany()
                .HasForeignKey(e => e.WorkflowId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.WorkflowVersion)
                .WithMany()
                .HasForeignKey(e => e.WorkflowVersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureWorkflowInstanceVariable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowInstanceVariable>(entity =>
        {
            entity.ToTable("WorkflowInstanceVariables");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Value).HasMaxLength(4000);
            entity.HasOne(e => e.WorkflowInstance)
                .WithMany(e => e.Variables)
                .HasForeignKey(e => e.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Variable)
                .WithMany()
                .HasForeignKey(e => e.VariableId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureWorkflowInstanceTransition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkflowInstanceTransition>(entity =>
        {
            entity.ToTable("WorkflowInstanceTransitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Result).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.PerformedAt).IsRequired();
            entity.HasOne(e => e.WorkflowInstance)
                .WithMany(e => e.Transitions)
                .HasForeignKey(e => e.WorkflowInstanceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.SourceNode)
                .WithMany()
                .HasForeignKey(e => e.SourceNodeId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.TargetNode)
                .WithMany()
                .HasForeignKey(e => e.TargetNodeId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    // Report Builder Configuration
    private void ConfigureReport(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("Reports");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ReportCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NameFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.SqlQuery).HasColumnType("nvarchar(max)");
            entity.Property(e => e.DesignData).HasColumnType("nvarchar(max)");
            entity.Property(e => e.FilterSettings).HasColumnType("nvarchar(max)");
            entity.Property(e => e.GroupSettings).HasColumnType("nvarchar(max)");
            entity.Property(e => e.SortSettings).HasColumnType("nvarchar(max)");
            entity.Property(e => e.FormatSettings).HasColumnType("nvarchar(max)");

            entity.HasIndex(e => e.ReportCode).IsUnique();

            entity.HasOne(e => e.Category)
                .WithMany(e => e.Reports)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Form)
                .WithMany()
                .HasForeignKey(e => e.FormId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureReportCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportCategory>(entity =>
        {
            entity.ToTable("ReportCategories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NameFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(100);

            entity.HasIndex(e => e.Code).IsUnique();

            entity.HasOne(e => e.ParentCategory)
                .WithMany(e => e.SubCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureReportField(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportField>(entity =>
        {
            entity.ToTable("ReportFields");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FieldName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TitleFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TitleEn).HasMaxLength(200);
            entity.Property(e => e.DataType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(500);
            entity.Property(e => e.FormatSettings).HasColumnType("nvarchar(max)");

            entity.HasOne(e => e.Report)
                .WithMany(e => e.Fields)
                .HasForeignKey(e => e.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureReportFilter(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportFilter>(entity =>
        {
            entity.ToTable("ReportFilters");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FieldName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TitleFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TitleEn).HasMaxLength(200);
            entity.Property(e => e.DefaultValue).HasMaxLength(1000);
            entity.Property(e => e.Settings).HasColumnType("nvarchar(max)");

            entity.HasOne(e => e.Report)
                .WithMany(e => e.Filters)
                .HasForeignKey(e => e.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureReportGroup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportGroup>(entity =>
        {
            entity.ToTable("ReportGroups");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FieldName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TitleFa).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TitleEn).HasMaxLength(200);

            entity.HasOne(e => e.Report)
                .WithMany(e => e.Groups)
                .HasForeignKey(e => e.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureReportSort(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportSort>(entity =>
        {
            entity.ToTable("ReportSorts");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FieldName).HasMaxLength(200).IsRequired();

            entity.HasOne(e => e.Report)
                .WithMany(e => e.Sorts)
                .HasForeignKey(e => e.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureReportOutput(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportOutput>(entity =>
        {
            entity.ToTable("ReportOutputs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(1000);
            entity.Property(e => e.Parameters).HasColumnType("nvarchar(max)");

            entity.HasOne(e => e.Report)
                .WithMany(e => e.Outputs)
                .HasForeignKey(e => e.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreationDate = DateTime.UtcNow;
                entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entity.EditDate = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}



