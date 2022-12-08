module.exports = {
    displayName: "Collate Example Mod",
    author: "Tomat",
    version: "2.0.0",
    homepage: "https://github.com/rejuvena/collate",
    
    side: "Both",
    sortBefore: "SomeModToSortBefore",
    sortAfter: "SomeModToSortAfter",
    
    hideCode: false,
    hidResources: false,
    includeSource: true,
    
    buildIgnore: ".collate/,Properties/,.accesstransformer,build.js"
}