# read spkl config
function LoadSpklConfig($filepath)
{
    $spklconfig = Get-Content -Raw -Path $filepath
    # remove /* */ comments 
    $spklconfig = $spklconfig -replace "/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/",""
    # remove // comments
    $spklconfig = $spklconfig -replace "\/\/.*[\r\n]",""
    # remove whitespace
    $spklconfig = $spklconfig -replace "\s*[\r\n]",""
    $spklconfig = $spklconfig | ConvertFrom-Json
    $spklconfig
    return
}

function GetWebResourceConfig()
{
    Write-Host "Reading spkl.json for web resources" -ForegroundColor Gray
    $config =New-Object –TypeName PSObject
    $basePath = "..\..\WebResources"
    $config | Add-Member –MemberType NoteProperty –Name RootPath –Value ((Get-Item $basePath).FullName + "\")
    $config | Add-Member –MemberType NoteProperty –Name SpklPath –Value ($basePath + "\spkl.json")
    $config | Add-Member –MemberType NoteProperty –Name SpklConfig –Value (LoadSpklConfig($config.SpklPath))
    $config | Add-Member –MemberType NoteProperty –Name Prefix -Value ($config.SpklConfig.webresources[0].files[0].file.Split('_')[0] + "_")
    $config | Add-Member –MemberType NoteProperty –Name Path –Value ($basePath + "\" + $config.Prefix)
    $config
    return
}

function AddFiles($config)
{
    $items = Get-ChildItem -Path $config.Path -rec | where {$_.extension -in ".htm",".html",".css",".js",".xml",".png",".jpg",".gif",".xap",".xsl",".xslt",".ico",".svg"}

    # build files array
    [System.Collections.ArrayList]$files = @()

    
    # add existing files in config
    $files.AddRange($config.SpklConfig.webresources[0].files)
    
    foreach ($item in $items)
    {
        if ($item.Attributes -ne "Directory")
        {
            $file = New-Object PSObject
            $filepath = $item.FullName.Replace($config.RootPath,"")
            $uniquename = $filepath.Replace("\","/")
        
            if ($config.SpklConfig.webresources[0].files.uniquename -contains $uniquename -ne "True") {
                Add-Member -InputObject $file -MemberType NoteProperty -Name file -Value $filepath
                Add-Member -InputObject $file -MemberType NoteProperty -Name uniquename -Value $uniquename
                Add-Member -InputObject $file -MemberType NoteProperty -Name description -Value ""
                $files.Add($file)
                Write-Host "Adding " -NoNewline -ForegroundColor Gray
                Write-Host $filepath -ForegroundColor Yellow
            }
        }
    }
    # Overwrite Files
    $config.SpklConfig.webresources[0].files = $files
    $config
    return
}

function UpdateConfig($config)
{
    Write-Host "Updating spkl.json" -ForegroundColor Gray
    ConvertTo-Json ($config.SpklConfig) -Depth 100 | Out-File $config.SpklPath -Force
    Write-Host "Updated!" -ForegroundColor Yellow
    Write-Host "" -ForegroundColor Gray
}

$updatedConfig = AddFiles(GetWebResourceConfig)
UpdateConfig($updatedConfig)