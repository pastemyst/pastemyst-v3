import type Editor from "$lib/Editor.svelte";

export default class TabData {
    id: string;
    title: string;
    isInRenamingState = false;
    editor: Editor;

    constructor(id: string, title: string, editor: Editor) {
        this.id = id;
        this.title = title;
        this.editor = editor;
    }
}
