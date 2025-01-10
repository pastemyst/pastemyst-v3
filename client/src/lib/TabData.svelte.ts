import type Editor from "$lib/Editor.svelte";

export default class TabData {
    id: string = $state("");
    title: string = $state("untitled");
    isInRenamingState = $state(false);
    editor?: ReturnType<typeof Editor> = $state();

    constructor(id: string, title: string, editor: Editor) {
        this.id = id;
        this.title = title;
        this.editor = editor;
    }
}
